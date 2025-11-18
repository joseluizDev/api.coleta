using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;
using api.minionStorage.Services;
using static System.Net.Mime.MediaTypeNames;

namespace api.coleta.Services
{
    public class RelatorioService : ServiceBase
    {
        private readonly RelatorioRepository _relatorioRepository;
        private readonly IMinioStorage _minioStorage;
        private readonly GeoJsonRepository _geoJsonRepository;

        public RelatorioService(RelatorioRepository relatorioRepository, IMinioStorage minioStorage, GeoJsonRepository geoJsonRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _relatorioRepository = relatorioRepository;
            _minioStorage = minioStorage;
            _geoJsonRepository = geoJsonRepository;
        }

        public async Task<string?> SalvarRelatorio(RelatorioDTO arquivo, Guid userId)
        {
            string bucketName = "coleta";
            var file = arquivo.Arquivo;

            string fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            string contentType = file.ContentType;
            string objectName = $"{Guid.NewGuid()}.{fileExtension}";

            using var stream = file.OpenReadStream();
            string url = await _minioStorage.UploadFileAsync(bucketName, objectName, stream, contentType);

            if (url != null)
            {
                Relatorio map = RelatorioMapDto.MapRelatorio(arquivo);
                map.LinkBackup = url;
                map.UsuarioId = userId;

                _relatorioRepository.Adicionar(map);
                UnitOfWork.Commit();
                return "Opa";

            }

            return url;
        }

        public async Task<RelatorioOuputDTO?> GetRelario(Guid id, Guid userId)
        {
            var relatorio = await _relatorioRepository.ObterPorId(id, userId);
            if (relatorio != null)
            {
                return RelatorioMapDto.MapRelatorio(relatorio);
            }
            return null;

        }

        public async Task<List<RelatorioOuputDTO>> ListarRelatoriosPorUploadAsync(Guid userId)
        {

            var relatorios = await _relatorioRepository.ListarRelatoriosPorUploadAsync(userId);
            if (relatorios == null || relatorios.Count == 0)
            {
                return [];
            }

            return relatorios.MapRelatorioSemJson(); // Excludes JsonRelatorio for performance
        }

        public async Task<bool> AtualizarJsonRelatorioAsync(Guid coletaId, Guid relatorioId, Guid userId, string jsonRelatorio)
        {
            var relatorio = await _relatorioRepository.ObterPorIdColetaRelatorio(coletaId, relatorioId, userId);
            if (relatorio == null)
            {
                return false;
            }

            relatorio.JsonRelatorio = jsonRelatorio;

            _relatorioRepository.Atualizar(relatorio);
            UnitOfWork.Commit();

            return true;
        }

        public async Task<RelatorioCompletoOutputDTO?> GetRelatorioCompletoAsync(Guid relatorioId, Guid userId)
        {
            var relatorio = await _relatorioRepository.ObterPorRelatorioId(relatorioId, userId);
            if (relatorio == null)
            {
                return null;
            }

            var relatorioDto = RelatorioMapDto.MapRelatorio(relatorio);
            
            // Montar dados da coleta com geojson processado
            ColetaDadosDTO? dadosColeta = null;
            
            if (relatorio.Coleta != null)
            {
                var coleta = relatorio.Coleta;
                var talhaoJson = coleta.Talhao;
                
                // Buscar geojson pelo ID da coleta
                var geojson = _geoJsonRepository.ObterPorId(coleta.GeojsonID);
                
                // Processar GeoJSON para extrair grid e pontos
                object? geoJsonProcessado = null;
                int zonas = 0;
                
                if (geojson != null && !string.IsNullOrEmpty(geojson.Pontos))
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var pontos = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(geojson.Pontos, options);
                        
                        var gridList = new List<object>();
                        var pointsList = new List<object>();
                        
                        // Extrair pontos
                        if (pontos.TryGetProperty("points", out var pointsElement))
                        {
                            foreach (var point in pointsElement.EnumerateArray())
                            {
                                if (point.TryGetProperty("geometry", out var geometry) &&
                                    geometry.TryGetProperty("coordinates", out var coordinates) &&
                                    point.TryGetProperty("properties", out var properties))
                                {
                                    pointsList.Add(new
                                    {
                                        dados = new
                                        {
                                            id = properties.TryGetProperty("id", out var id) ? id.GetInt32() : 1,
                                            hexagonId = properties.TryGetProperty("hexagonId", out var hexId) ? hexId.GetInt32() : 1,
                                            coletado = properties.TryGetProperty("coletado", out var coletado) ? coletado.GetBoolean() : false
                                        },
                                        cordenadas = new[] { coordinates[0].GetDouble(), coordinates[1].GetDouble() }
                                    });
                                }
                            }
                        }
                        
                        // Extrair grid (polígonos)
                        if (pontos.TryGetProperty("features", out var featuresElement))
                        {
                            foreach (var feature in featuresElement.EnumerateArray())
                            {
                                if (feature.TryGetProperty("geometry", out var geometry) &&
                                    geometry.TryGetProperty("type", out var geoType) &&
                                    geoType.GetString() == "Polygon" &&
                                    geometry.TryGetProperty("coordinates", out var coordinates))
                                {
                                    try
                                    {
                                        var coords = System.Text.Json.JsonSerializer.Deserialize<List<List<double[]>>>(coordinates.GetRawText(), options);
                                        if (coords != null && coords.Count > 0 && coords[0].Count > 0)
                                        {
                                            gridList.Add(new { cordenadas = coords[0] });
                                            zonas++;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        
                        geoJsonProcessado = new
                        {
                            grid = gridList,
                            points = pointsList
                        };
                    }
                    catch { }
                }
                
                dadosColeta = new ColetaDadosDTO
                {
                    ColetaId = coleta.Id,
                    Geojson = geoJsonProcessado,
                    Talhao = talhaoJson != null ? new
                    {
                        id = talhaoJson.Id,
                        nome = talhaoJson.Nome,
                        area = talhaoJson.Area,
                        observacao = talhaoJson.Observacao,
                        coordenadas = talhaoJson.Coordenadas
                    } : null,
                    UsuarioResp = coleta.UsuarioResp != null ? new
                    {
                        id = coleta.UsuarioResp.Id,
                        nomeCompleto = coleta.UsuarioResp.NomeCompleto,
                        cpf = coleta.UsuarioResp.CPF,
                        email = coleta.UsuarioResp.Email,
                        telefone = coleta.UsuarioResp.Telefone
                    } : null,
                    GeoJsonID = coleta.GeojsonID,
                    UsuarioRespID = coleta.UsuarioRespID,
                    Zonas = zonas,
                    AreaHa = !string.IsNullOrEmpty(talhaoJson?.Area) && decimal.TryParse(talhaoJson.Area, out var area) ? area : (decimal?)null
                };
            }
            
            return new RelatorioCompletoOutputDTO
            {
                Id = relatorioDto.Id,
                ColetaId = relatorioDto.ColetaId,
                LinkBackup = relatorioDto.LinkBackup,
                DataInclusao = relatorioDto.DataInclusao,
                NomeColeta = relatorioDto.NomeColeta,
                Talhao = relatorioDto.Talhao,
                TipoColeta = relatorioDto.TipoColeta,
                Fazenda = relatorioDto.Fazenda,
                NomeCliente = relatorioDto.NomeCliente,
                Safra = relatorioDto.Safra,
                Funcionario = relatorioDto.Funcionario,
                Observacao = relatorioDto.Observacao,
                Profundidade = relatorioDto.Profundidade,
                TiposAnalise = relatorioDto.TiposAnalise,
                JsonRelatorio = relatorioDto.JsonRelatorio,
                IsRelatorio = relatorioDto.IsRelatorio,
                DadosColeta = dadosColeta
            };
        }
    }
}
