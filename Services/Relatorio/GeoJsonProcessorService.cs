using System.Text.Json;
using api.coleta.Repositories;

namespace api.coleta.Services.Relatorio
{
    /// <summary>
    /// Serviço responsável pelo processamento de dados GeoJSON.
    /// Extrai grid (hexágonos) e pontos de coleta do GeoJSON.
    /// </summary>
    public class GeoJsonProcessorService
    {
        private readonly GeoJsonRepository _geoJsonRepository;

        public GeoJsonProcessorService(GeoJsonRepository geoJsonRepository)
        {
            _geoJsonRepository = geoJsonRepository;
        }

        /// <summary>
        /// Processa GeoJSON para extrair grid e pontos de coleta
        /// </summary>
        /// <param name="geojsonId">ID do GeoJSON no banco</param>
        /// <param name="zonas">Output: número de zonas (hexágonos) encontradas</param>
        /// <returns>Objeto processado com grid e points, ou null se não houver dados</returns>
        public object? ProcessarGeoJson(Guid geojsonId, out int zonas)
        {
            zonas = 0;

            var geojson = _geoJsonRepository.ObterPorId(geojsonId);
            if (geojson == null || string.IsNullOrEmpty(geojson.Pontos))
            {
                return null;
            }

            return ProcessarGeoJsonString(geojson.Pontos, out zonas);
        }

        /// <summary>
        /// Processa uma string GeoJSON diretamente
        /// </summary>
        /// <param name="pontosJson">String JSON com os dados GeoJSON</param>
        /// <param name="zonas">Output: número de zonas encontradas</param>
        /// <returns>Objeto processado com grid e points</returns>
        public object? ProcessarGeoJsonString(string? pontosJson, out int zonas)
        {
            zonas = 0;

            if (string.IsNullOrEmpty(pontosJson))
            {
                return null;
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var pontos = JsonSerializer.Deserialize<JsonElement>(pontosJson, options);

                var gridList = ExtrairGrid(pontos, ref zonas);
                var pointsList = ExtrairPontos(pontos);

                return new
                {
                    grid = gridList,
                    points = pointsList
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extrai lista de polígonos do grid (hexágonos) do GeoJSON
        /// </summary>
        private List<object> ExtrairGrid(JsonElement pontos, ref int zonas)
        {
            var gridList = new List<object>();

            if (!pontos.TryGetProperty("features", out var featuresElement))
            {
                return gridList;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var feature in featuresElement.EnumerateArray())
            {
                if (!feature.TryGetProperty("geometry", out var geometry))
                    continue;

                if (!geometry.TryGetProperty("type", out var geoType))
                    continue;

                if (geoType.GetString() != "Polygon")
                    continue;

                if (!geometry.TryGetProperty("coordinates", out var coordinates))
                    continue;

                try
                {
                    var coords = JsonSerializer.Deserialize<List<List<double[]>>>(
                        coordinates.GetRawText(), options);

                    if (coords != null && coords.Count > 0 && coords[0].Count > 0)
                    {
                        gridList.Add(new { cordenadas = coords[0] });
                        zonas++;
                    }
                }
                catch
                {
                    // Ignora polígonos com formato inválido
                }
            }

            return gridList;
        }

        /// <summary>
        /// Extrai lista de pontos de coleta do GeoJSON
        /// </summary>
        private List<object> ExtrairPontos(JsonElement pontos)
        {
            var pointsList = new List<object>();

            if (!pontos.TryGetProperty("points", out var pointsElement))
            {
                return pointsList;
            }

            foreach (var point in pointsElement.EnumerateArray())
            {
                if (!point.TryGetProperty("geometry", out var geometry))
                    continue;

                if (!geometry.TryGetProperty("coordinates", out var coordinates))
                    continue;

                if (!point.TryGetProperty("properties", out var properties))
                    continue;

                try
                {
                    pointsList.Add(new
                    {
                        dados = new
                        {
                            id = properties.TryGetProperty("id", out var id) ? id.GetInt32() : 1,
                            hexagonId = properties.TryGetProperty("hexagonId", out var hexId) ? hexId.GetInt32() : 1,
                            coletado = properties.TryGetProperty("coletado", out var coletado) && coletado.GetBoolean()
                        },
                        cordenadas = new[] { coordinates[0].GetDouble(), coordinates[1].GetDouble() }
                    });
                }
                catch
                {
                    // Ignora pontos com formato inválido
                }
            }

            return pointsList;
        }
    }
}
