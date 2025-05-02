using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.funcionario.Models.DTOs;
using api.talhao.Models.DTOs;
using api.talhao.Services;
using AutoMapper;
using api.cliente.Models.DTOs;
using System.Text.Json;
using System.Collections.Generic;

namespace api.coleta.Services
{
    public class VisualizarMapaService : ServiceBase
    {
        private readonly VisualizarMapaRepository _visualizarMapaRepository;
        private readonly GeoJsonRepository _geoJsonRepository;
        private readonly UsuarioService _usuarioService;
        private readonly TalhaoService _talhaoService;
        public VisualizarMapaService(
            UsuarioService usuarioService,
            VisualizarMapaRepository visualizarMapaRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            GeoJsonRepository geoJsonRepository,
            TalhaoService talhaoService)
            : base(unitOfWork, mapper)
        {
            _visualizarMapaRepository = visualizarMapaRepository;
            _geoJsonRepository = geoJsonRepository;
            _talhaoService = talhaoService;
            _usuarioService = usuarioService;
        }

        public VisualizarMapOutputDto? Salvar(Guid userID, VisualizarMapInputDto visualizarMapa)
        {
            Geojson montarJson = new Geojson
            {
                Pontos = visualizarMapa.Geojson.ToString(),
                Grid = "1"
            };
            Geojson? retorno = _geoJsonRepository.Adicionar(montarJson);
            if (retorno != null)
            {
                visualizarMapa.GeojsonId = retorno.Id;
                var map = VisualizarDto.MapVisualizar(visualizarMapa);
                map.UsuarioID = userID;
                _visualizarMapaRepository.SalvarVisualizarMapa(map);
                if (UnitOfWork.Commit())
                {
                    return _mapper.Map<VisualizarMapOutputDto>(map);
                }
            }

            return null;
        }

        public PagedResult<VisualizarMapOutputDto?> Listar(Guid userID, int page)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapa(userID, page);
            if (visualizarMapa == null)
            {
                return new PagedResult<VisualizarMapOutputDto?>
                {
                    Items = [],
                    TotalPages = 0,
                    CurrentPage = page
                };
            }

            var mappedItems = _mapper.Map<List<VisualizarMapOutputDto?>>(visualizarMapa.Items);

            foreach (var coleta in mappedItems)
            {
                if (coleta == null) continue;

                var talhao = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhao != null)
                {
                    coleta.Talhao = _mapper.Map<Talhoes>(talhao);
                }

                var funcionario = _usuarioService.BuscarUsuarioPorId(coleta.UsuarioRespID);
                if (funcionario != null)
                {
                    coleta.UsuarioResp = _mapper.Map<UsuarioResponseDTO>(funcionario);
                }
            }

            return new PagedResult<VisualizarMapOutputDto?>
            {
                Items = mappedItems,
                TotalPages = visualizarMapa.TotalPages,
                CurrentPage = visualizarMapa.CurrentPage
            };
        }

        public bool? ExcluirColeta(Guid userID, Guid id)
        {
            Coleta buscarColeta = _visualizarMapaRepository.BuscarVisualizarMapaPorIdTalhao(userID, id);
            if (buscarColeta != null)
            {
                _visualizarMapaRepository.DeletarVisualizarMapa(buscarColeta);
                UnitOfWork.Commit();
                return true;
            }
            return false;
        }

        public VisualizarMapOutputDto? BuscarVisualizarMapaPorId(Guid userId, Guid id)
        {
            var visualizarMapa = _visualizarMapaRepository.BuscarVisualizarMapaPorId(userId, id);
            if (visualizarMapa != null)
            {
                var mappedItem = _mapper.Map<VisualizarMapOutputDto>(visualizarMapa);
                var talhao = _talhaoService.BuscarTalhaoJsonPorId(mappedItem.TalhaoID);
                if (talhao != null)
                {
                    mappedItem.Talhao = _mapper.Map<Talhoes>(talhao);
                }

                var geojson = _geoJsonRepository.ObterPorId(mappedItem.GeoJsonID);
                if (geojson != null)
                {
                    mappedItem.Geojson = geojson;
                }

                var funcionario = _usuarioService.BuscarUsuarioPorId(mappedItem.UsuarioRespID);
                if (funcionario != null)
                {
                    mappedItem.UsuarioResp = _mapper.Map<UsuarioResponseDTO>(funcionario);
                }
                return mappedItem;
            }
            return null;
        }

        public List<VisualizarMapOutputDto?> ListarMobile(Guid userID)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapaMobile(userID);
            if (visualizarMapa == null || visualizarMapa.Count == 0)
            {
                return [];
            }

            var mappedItems = _mapper.Map<List<VisualizarMapOutputDto?>>(visualizarMapa);

            foreach (var coleta in mappedItems)
            {
                if (coleta == null) continue;

                var talhao = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhao != null)
                {
                    coleta.Talhao = _mapper.Map<Talhoes>(talhao);
                }

                var funcionario = _usuarioService.BuscarUsuarioPorId(coleta.UsuarioRespID);
                if (funcionario != null)
                {
                    coleta.UsuarioResp = _mapper.Map<UsuarioResponseDTO>(funcionario);
                }

                var geojson = _geoJsonRepository.ObterPorId(coleta.GeoJsonID);
                if (geojson != null)
                {
                    coleta.Geojson = _mapper.Map<Geojson>(geojson);
                }
            }

            return mappedItems;
        }

        public List<object> ListarMobilePorFazenda(Guid userID)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapaMobile(userID);
            if (visualizarMapa == null || visualizarMapa.Count == 0)
            {
                return [];
            }

            var mappedItems = _mapper.Map<List<VisualizarMapOutputDto>>(visualizarMapa);
            var result = new List<object>();

            foreach (var coleta in mappedItems)
            {
                if (coleta == null) continue;


                var talhaoJson = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhaoJson == null) continue;

                coleta.Talhao = _mapper.Map<Talhoes>(talhaoJson);


                var funcionario = _usuarioService.BuscarUsuarioPorId(coleta.UsuarioRespID);
                if (funcionario == null) continue;

                coleta.UsuarioResp = _mapper.Map<UsuarioResponseDTO>(funcionario);


                var geojson = _geoJsonRepository.ObterPorId(coleta.GeoJsonID);
                if (geojson == null) continue;

                coleta.Geojson = _mapper.Map<Geojson>(geojson);


                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


                var geoJsonData = new
                {
                    grid = new List<object>(),
                    points = new List<object>()
                };


                if (!string.IsNullOrEmpty(coleta.Geojson.Pontos))
                {
                    try
                    {
                        var pontos = JsonSerializer.Deserialize<JsonElement>(coleta.Geojson.Pontos, options);


                        if (pontos.TryGetProperty("points", out JsonElement pointsElement))
                        {
                            var pointsList = new List<object>();
                            foreach (var point in pointsElement.EnumerateArray())
                            {

                                if (point.TryGetProperty("geometry", out JsonElement geometry) &&
                                    geometry.TryGetProperty("coordinates", out JsonElement coordinates) &&
                                    point.TryGetProperty("properties", out JsonElement properties))
                                {
                                    var pointData = new
                                    {
                                        dados = new
                                        {
                                            id = properties.TryGetProperty("id", out JsonElement id) ? id.GetInt32() : 1,
                                            hexagonId = properties.TryGetProperty("hexagonId", out JsonElement hexId) ? hexId.GetInt32() : 1,
                                            coletado = properties.TryGetProperty("coletado", out JsonElement coletado) ? coletado.GetBoolean() : false
                                        },
                                        cordenadas = new double[]
                                        {
                                            coordinates[0].GetDouble(),
                                            coordinates[1].GetDouble()
                                        }
                                    };
                                    pointsList.Add(pointData);
                                }
                            }
                            geoJsonData.points.AddRange(pointsList);
                        }


                        if (pontos.TryGetProperty("features", out JsonElement featuresElement))
                        {
                            try
                            {
                                var features = featuresElement.EnumerateArray();
                                var polygons = new List<JsonElement>();


                                foreach (var feature in features)
                                {
                                    if (feature.TryGetProperty("geometry", out JsonElement geometry) &&
                                        geometry.TryGetProperty("type", out JsonElement geoType) &&
                                        geoType.GetString() == "Polygon" &&
                                        geometry.TryGetProperty("coordinates", out JsonElement coordinates))
                                    {
                                        polygons.Add(coordinates);
                                    }
                                }

                                if (polygons.Count > 0)
                                {

                                    foreach (var polygon in polygons)
                                    {
                                        try
                                        {

                                            var coordinates = JsonSerializer.Deserialize<List<List<double[]>>>(polygon.GetRawText(), options);
                                            if (coordinates != null && coordinates.Count > 0 && coordinates[0].Count > 0)
                                            {
                                                var gridData = new
                                                {
                                                    cordenadas = coordinates[0]
                                                };
                                                geoJsonData.grid.Add(gridData);
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(coleta.Geojson.Grid) && coleta.Geojson.Grid != "1")
                                {

                                    var gridJson = JsonSerializer.Deserialize<JsonElement>(coleta.Geojson.Grid, options);
                                    if (gridJson.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var gridItem in gridJson.EnumerateArray())
                                        {
                                            if (gridItem.TryGetProperty("coordinates", out JsonElement coords))
                                            {
                                                try
                                                {

                                                    var coordinates = JsonSerializer.Deserialize<List<List<double[]>>>(coords.GetRawText(), options);
                                                    if (coordinates != null && coordinates.Count > 0 && coordinates[0].Count > 0)
                                                    {
                                                        var gridData = new
                                                        {
                                                            cordenadas = coordinates[0]
                                                        };
                                                        geoJsonData.grid.Add(gridData);
                                                    }
                                                }
                                                catch
                                                {

                                                    try
                                                    {
                                                        var coordinates = JsonSerializer.Deserialize<List<double[]>>(coords.GetRawText(), options);
                                                        if (coordinates != null && coordinates.Count > 0)
                                                        {
                                                            var gridData = new
                                                            {
                                                                cordenadas = coordinates
                                                            };
                                                            geoJsonData.grid.Add(gridData);
                                                        }
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (coleta.Talhao?.Coordenadas != null)
                                {

                                    try
                                    {
                                        var coordsList = new List<double[]>();
                                        var talhaoCoords = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(coleta.Talhao.Coordenadas), options);

                                        if (talhaoCoords.ValueKind == JsonValueKind.Array)
                                        {
                                            foreach (var coord in talhaoCoords.EnumerateArray())
                                            {
                                                if (coord.TryGetProperty("lat", out JsonElement lat) &&
                                                    coord.TryGetProperty("lng", out JsonElement lng))
                                                {
                                                    coordsList.Add(new[] { lng.GetDouble(), lat.GetDouble() });
                                                }
                                            }

                                            if (coordsList.Count > 0)
                                            {

                                                coordsList.Add(coordsList[0]);

                                                var gridData = new
                                                {
                                                    cordenadas = coordsList
                                                };
                                                geoJsonData.grid.Add(gridData);
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch
                    {

                        if (coleta.Talhao?.Coordenadas != null)
                        {
                            try
                            {

                                var coordsList = new List<double[]>();

                                try
                                {

                                    var talhaoCoords = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(coleta.Talhao.Coordenadas), options);
                                    if (talhaoCoords.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var coord in talhaoCoords.EnumerateArray())
                                        {
                                            if (coord.TryGetProperty("lat", out JsonElement lat) &&
                                                coord.TryGetProperty("lng", out JsonElement lng))
                                            {
                                                coordsList.Add(new[] { lng.GetDouble(), lat.GetDouble() });
                                            }
                                        }
                                    }
                                }
                                catch
                                {

                                }

                                if (coordsList.Count > 0)
                                {

                                    double sumLat = 0, sumLng = 0;
                                    foreach (var coord in coordsList)
                                    {
                                        sumLng += coord[0];
                                        sumLat += coord[1];
                                    }

                                    double avgLng = sumLng / coordsList.Count;
                                    double avgLat = sumLat / coordsList.Count;


                                    var pointData = new
                                    {
                                        dados = new
                                        {
                                            id = 1,
                                            hexagonId = 1,
                                            coletado = false
                                        },
                                        cordenadas = new[] { avgLng, avgLat }
                                    };
                                    geoJsonData.points.Add(pointData);


                                    coordsList.Add(coordsList[0]);


                                    var gridData = new
                                    {
                                        cordenadas = coordsList
                                    };
                                    geoJsonData.grid.Add(gridData);
                                }
                                else
                                {


                                }
                            }
                            catch
                            {


                            }
                        }
                        else
                        {


                        }
                    }
                }



                var item = new
                {
                    id = coleta.Id,
                    talhao = coleta.Talhao != null ? new
                    {
                        id = coleta.Talhao.Id,
                        area = coleta.Talhao.Area,
                        nome = coleta.Talhao.Nome,
                        observacao = coleta.Talhao.observacao,
                        talhaoID = coleta.Talhao.TalhaoID,
                        coordenadas = coleta.Talhao.Coordenadas
                    } : null,
                    geojson = geoJsonData,
                    geoJsonID = coleta.GeoJsonID,
                    usuarioResp = coleta.UsuarioResp != null ? new
                    {
                        id = coleta.UsuarioResp.Id,
                        nomeCompleto = coleta.UsuarioResp.NomeCompleto,
                        cpf = coleta.UsuarioResp.CPF,
                        email = coleta.UsuarioResp.Email,
                        telefone = coleta.UsuarioResp.Telefone
                    } : null,
                    usuarioRespID = coleta.UsuarioRespID,
                    observacao = coleta.Observacao ?? "",
                    tipoColeta = coleta.TipoColeta,
                    tipoAnalise = coleta.TipoAnalise,
                    profundidade = coleta.Profundidade
                };

                result.Add(item);
            }

            return result;
        }

        public bool? SalvarColeta(Guid id, ColetaMobileDTO coleta)
        {
            Coleta? co = _visualizarMapaRepository.ObterVisualizarMapaPorId(coleta.ColetaID, id);
            if (co != null)
            {
                coleta.FuncionarioID = id;
                Geojson? geo = _geoJsonRepository.ObterPorId(co.GeojsonID);
                if (geo != null)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var geoJson = JsonSerializer.Deserialize<JsonElement>(geo.Pontos, options);


                    var featuresElement = geoJson.GetProperty("features");


                    var points = geoJson.GetProperty("points").EnumerateArray()
                        .Select(p =>
                        {
                            try
                            {

                                if (p.TryGetProperty("dados", out JsonElement dadosElement))
                                {

                                    var id = dadosElement.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt32() : 0;


                                    if (coleta.Ponto != null && id == coleta.Ponto.Id)
                                    {

                                        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(new
                                        {
                                            dados = new
                                            {
                                                id = id,
                                                hexagonId = dadosElement.TryGetProperty("hexagonId", out JsonElement hexIdElement) ? hexIdElement.GetInt32() : 1,
                                                coletado = true
                                            },
                                            cordenadas = p.TryGetProperty("cordenadas", out JsonElement coordsElement) ?
                                                JsonSerializer.Deserialize<double[]>(coordsElement.GetRawText()) :
                                                new double[] { 0, 0 }
                                        }));
                                    }
                                }

                                return p;
                            }
                            catch
                            {

                                return p;
                            }
                        })
                        .ToArray();


                    var novoGeoJson = new
                    {
                        type = geoJson.GetProperty("type").GetString(),
                        features = JsonSerializer.Deserialize<object>(featuresElement.GetRawText(), options),
                        points = points
                    };

                    geo.Pontos = JsonSerializer.Serialize(novoGeoJson, options);
                    _geoJsonRepository.Atualizar(geo);
                    UnitOfWork.Commit();
                    return true;
                }
            }
            return false;
        }

    }
}
