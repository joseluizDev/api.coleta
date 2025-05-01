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

                // Carregar dados do talhão
                var talhaoJson = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhaoJson == null) continue;

                coleta.Talhao = _mapper.Map<Talhoes>(talhaoJson);

                // Carregar dados do usuário responsável
                var funcionario = _usuarioService.BuscarUsuarioPorId(coleta.UsuarioRespID);
                if (funcionario == null) continue;

                coleta.UsuarioResp = _mapper.Map<UsuarioResponseDTO>(funcionario);

                // Carregar dados do geojson
                var geojson = _geoJsonRepository.ObterPorId(coleta.GeoJsonID);
                if (geojson == null) continue;

                coleta.Geojson = _mapper.Map<Geojson>(geojson);

                // Criar estrutura personalizada para o retorno
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Processar o GeoJson para o formato desejado
                var geoJsonData = new
                {
                    grid = new List<object>(),
                    points = new List<object>()
                };

                // Tentar processar o GeoJson se existir
                if (!string.IsNullOrEmpty(coleta.Geojson.Pontos))
                {
                    try
                    {
                        var pontos = JsonSerializer.Deserialize<JsonElement>(coleta.Geojson.Pontos, options);

                        // Processar pontos se existirem
                        if (pontos.TryGetProperty("points", out JsonElement pointsElement))
                        {
                            var pointsList = new List<object>();
                            foreach (var point in pointsElement.EnumerateArray())
                            {
                                // Extrair as coordenadas e dados do ponto
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

                        // Processar grid a partir dos features do GeoJSON
                        if (pontos.TryGetProperty("features", out JsonElement featuresElement))
                        {
                            try
                            {
                                var features = featuresElement.EnumerateArray();
                                var polygons = new List<JsonElement>();

                                // Extrair todos os polígonos dos features
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
                                    // Usar os polígonos encontrados para o grid
                                    foreach (var polygon in polygons)
                                    {
                                        try
                                        {
                                            // Extrair as coordenadas do polígono (primeiro nível)
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
                                            // Se falhar ao processar o polígono, ignora e continua
                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(coleta.Geojson.Grid) && coleta.Geojson.Grid != "1")
                                {
                                    // Se não encontrou polígonos nos features, tenta usar o Grid
                                    var gridJson = JsonSerializer.Deserialize<JsonElement>(coleta.Geojson.Grid, options);
                                    if (gridJson.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var gridItem in gridJson.EnumerateArray())
                                        {
                                            if (gridItem.TryGetProperty("coordinates", out JsonElement coords))
                                            {
                                                try
                                                {
                                                    // Tentar extrair as coordenadas diretamente
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
                                                    // Se falhar, tenta outro formato
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
                                                        // Se falhar novamente, ignora e continua
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (coleta.Talhao?.Coordenadas != null)
                                {
                                    // Se não encontrou grid, tenta usar as coordenadas do talhão
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
                                                // Adicionar o primeiro ponto novamente para fechar o polígono
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
                                        // Falha ao processar coordenadas do talhão
                                    }
                                }
                            }
                            catch
                            {
                                // Falha ao processar features
                            }
                        }
                    }
                    catch
                    {
                        // Se falhar ao processar o GeoJson, tenta usar as coordenadas do talhão
                        if (coleta.Talhao?.Coordenadas != null)
                        {
                            try
                            {
                                // Tenta extrair as coordenadas do talhão
                                var coordsList = new List<double[]>();

                                try
                                {
                                    // Tenta deserializar as coordenadas do talhão
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
                                    // Falha ao deserializar as coordenadas
                                }

                                if (coordsList.Count > 0)
                                {
                                    // Criar um ponto central para o points
                                    double sumLat = 0, sumLng = 0;
                                    foreach (var coord in coordsList)
                                    {
                                        sumLng += coord[0];
                                        sumLat += coord[1];
                                    }

                                    double avgLng = sumLng / coordsList.Count;
                                    double avgLat = sumLat / coordsList.Count;

                                    // Adicionar um ponto no centro
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

                                    // Adicionar o primeiro ponto novamente para fechar o polígono
                                    coordsList.Add(coordsList[0]);

                                    // Criar o grid com as coordenadas do talhão
                                    var gridData = new
                                    {
                                        cordenadas = coordsList
                                    };
                                    geoJsonData.grid.Add(gridData);
                                }
                                else
                                {
                                    // Se não conseguiu extrair coordenadas, não adiciona pontos ou grid
                                    // Deixa os arrays vazios para o cliente mobile lidar
                                }
                            }
                            catch
                            {
                                // Se falhar, não adiciona pontos ou grid
                                // Deixa os arrays vazios para o cliente mobile lidar
                            }
                        }
                        else
                        {
                            // Se não tiver coordenadas do talhão, não adiciona pontos ou grid
                            // Deixa os arrays vazios para o cliente mobile lidar
                        }
                    }
                }


                // Criar o objeto final no formato desejado
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

                    // Copia a propriedade features
                    var featuresElement = geoJson.GetProperty("features");

                    // Copia os pontos, atualizando o coletado quando encontrar o ponto do DTO
                    var points = geoJson.GetProperty("points").EnumerateArray()
                        .Select(p =>
                        {
                            var ponto = JsonSerializer.Deserialize<PontoDto>(p.GetRawText(), options);
                            if (ponto?.Properties != null && coleta.Ponto?.Properties != null &&
                                ponto.Properties.Id == coleta.Ponto.Properties.Id)
                            {
                                ponto.Properties.Coletado = true;
                            }
                            return ponto;
                        })
                        .ToArray();

                    // Reconstroi o objeto GeoJSON com features + points atualizados
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
