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
using api.safra.Services;
using api.safra.Models.DTOs;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace api.coleta.Services
{
    public class VisualizarMapaService : ServiceBase
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        private readonly VisualizarMapaRepository _visualizarMapaRepository;
        private readonly GeoJsonRepository _geoJsonRepository;
        private readonly UsuarioService _usuarioService;
        private readonly TalhaoService _talhaoService;
        private readonly SafraService _safraService;
        private readonly PontoColetadoRepository _pontoColetadoRepository;
        public VisualizarMapaService(
            UsuarioService usuarioService,
            VisualizarMapaRepository visualizarMapaRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            GeoJsonRepository geoJsonRepository,
            TalhaoService talhaoService,
            SafraService safraService,
            PontoColetadoRepository pontoColetadoRepository)
            : base(unitOfWork, mapper)
        {
            _visualizarMapaRepository = visualizarMapaRepository;
            _geoJsonRepository = geoJsonRepository;
            _talhaoService = talhaoService;
            _usuarioService = usuarioService;
            _safraService = safraService;
            _pontoColetadoRepository = pontoColetadoRepository;
        }

        public VisualizarMapOutputDto? Salvar(Guid userID, VisualizarMapInputDto visualizarMapa)
        {
            try
            {
                // Validação do GeoJSON
                if (visualizarMapa.Geojson.ValueKind == System.Text.Json.JsonValueKind.Undefined || 
                    visualizarMapa.Geojson.ValueKind == System.Text.Json.JsonValueKind.Null)
                {
                    throw new ArgumentException("GeoJSON é obrigatório e deve ser um objeto JSON válido.");
                }

                // Criar o objeto GeoJSON
                Geojson montarJson = new Geojson
                {
                    Pontos = visualizarMapa.Geojson.ToString(),
                    Grid = "1"
                };

                Console.WriteLine($"Tentando salvar GeoJSON com {montarJson.Pontos.Length} caracteres");

                // Salvar GeoJSON
                Geojson? retorno = _geoJsonRepository.Adicionar(montarJson);
                if (retorno == null)
                {
                    throw new InvalidOperationException("Falha ao salvar o GeoJSON no banco de dados.");
                }

                Console.WriteLine($"GeoJSON salvo com ID: {retorno.Id}");

                // Configurar o ID do GeoJSON no DTO
                visualizarMapa.GeojsonId = retorno.Id;

                // Mapear para entidade Coleta
                var map = VisualizarDto.MapVisualizar(visualizarMapa);
                map.UsuarioID = userID;

                Console.WriteLine($"Mapeamento concluído. TalhaoID: {map.TalhaoID}, UsuarioRespID: {map.UsuarioRespID}, UsuarioID: {map.UsuarioID}");

                // Salvar a coleta
                _visualizarMapaRepository.SalvarVisualizarMapa(map);

                Console.WriteLine("Coleta adicionada ao contexto, tentando commit...");

                // Commit da transação
                if (UnitOfWork.Commit())
                {
                    Console.WriteLine("Commit realizado com sucesso");
                    return _mapper.Map<VisualizarMapOutputDto>(map);
                }
                else
                {
                    throw new InvalidOperationException("Falha ao confirmar a transação no banco de dados.");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Erro de validação no enum: {ex.Message}");
                throw new ArgumentException($"Erro de validação: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado no serviço Salvar: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public VisualizarMapOutputDto? Atualizar(Guid userID, Guid id, VisualizarMapInputDto visualizarMapa)
        {
            try
            {
                // Buscar a coleta existente
                var coletaExistente = _visualizarMapaRepository.BuscarVisualizarMapaPorId(userID, id);
                if (coletaExistente == null)
                {
                    throw new ArgumentException("Visualização de mapa não encontrada ou não pertence ao usuário.");
                }

                // Validação do GeoJSON se foi informado
                if (visualizarMapa.Geojson.ValueKind != System.Text.Json.JsonValueKind.Undefined && 
                    visualizarMapa.Geojson.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    // Buscar o GeoJSON existente
                    var geoJsonExistente = _geoJsonRepository.ObterPorId(coletaExistente.GeojsonID);
                    if (geoJsonExistente != null)
                    {
                        // Atualizar o GeoJSON
                        geoJsonExistente.Pontos = visualizarMapa.Geojson.ToString();
                        _geoJsonRepository.Atualizar(geoJsonExistente);
                    }
                }

                // Atualizar os campos da coleta
                if (!string.IsNullOrEmpty(visualizarMapa.NomeColeta))
                    coletaExistente.NomeColeta = visualizarMapa.NomeColeta;
                
                if (visualizarMapa.TalhaoID != Guid.Empty)
                    coletaExistente.TalhaoID = visualizarMapa.TalhaoID;
                
                if (visualizarMapa.FuncionarioID != Guid.Empty)
                    coletaExistente.UsuarioRespID = visualizarMapa.FuncionarioID;
                
                if (!string.IsNullOrEmpty(visualizarMapa.TipoColeta))
                {
                    if (Enum.TryParse<TipoColeta>(visualizarMapa.TipoColeta, out var tipoColeta))
                        coletaExistente.TipoColeta = tipoColeta;
                }
                
                if (visualizarMapa.TipoAnalise != null && visualizarMapa.TipoAnalise.Any())
                {
                    var tiposAnaliseValidos = new List<TipoAnalise>();
                    foreach (var tipo in visualizarMapa.TipoAnalise)
                    {
                        if (Enum.TryParse<TipoAnalise>(tipo, out var tipoAnalise))
                            tiposAnaliseValidos.Add(tipoAnalise);
                    }
                    coletaExistente.TipoAnalise = tiposAnaliseValidos;
                }
                
                if (!string.IsNullOrEmpty(visualizarMapa.Profundidade))
                {
                    if (Enum.TryParse<Profundidade>(visualizarMapa.Profundidade, out var profundidade))
                        coletaExistente.Profundidade = profundidade;
                }
                
                if (!string.IsNullOrEmpty(visualizarMapa.Observacao))
                    coletaExistente.Observacao = visualizarMapa.Observacao;

                Console.WriteLine($"Atualizando coleta com ID: {coletaExistente.Id}");

                // Atualizar a coleta no repositório
                _visualizarMapaRepository.AtualizarVisualizarMapa(coletaExistente);

                Console.WriteLine("Coleta atualizada no contexto, tentando commit...");

                // Commit da transação
                if (UnitOfWork.Commit())
                {
                    Console.WriteLine("Commit da atualização realizado com sucesso");
                    return _mapper.Map<VisualizarMapOutputDto>(coletaExistente);
                }
                else
                {
                    throw new InvalidOperationException("Falha ao confirmar a transação de atualização no banco de dados.");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Erro de validação na atualização: {ex.Message}");
                throw new ArgumentException($"Erro de validação: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado no serviço Atualizar: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public PagedResult<VisualizarMapOutputDto?> Listar(Guid userID, QueryVisualizarMap query)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapa(userID, query);
            if (visualizarMapa == null)
            {
                return new PagedResult<VisualizarMapOutputDto?>
                {
                    Items = [],
                    TotalPages = 0,
                    CurrentPage = query.Page ?? 1
                };
            }

            var mappedItems = _mapper.Map<List<VisualizarMapOutputDto?>>(visualizarMapa.Items);

            foreach (var coleta in mappedItems)
            {
                if (coleta == null) continue;

                // Carregar Talhão
                var talhao = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhao != null)
                {
                    coleta.Talhao = _mapper.Map<Talhoes>(talhao);
                }

                // Carregar Safra
                if (coleta.SafraID.HasValue)
                {
                    var safra = _safraService.BuscarSafraPorId(null, coleta.SafraID.Value);
                    if (safra != null)
                    {
                        coleta.Safra = _mapper.Map<Safra>(safra);
                    }
                }

                // Carregar Funcionário
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

                    // Preencher FazendaID e ClienteID baseado no talhão relacionado (se existir)
                    try
                    {
                        var talhaoRelacionado = _talhaoService.BuscarTalhaoPorTalhaoJson(mappedItem.TalhaoID);
                        if (talhaoRelacionado != null)
                        {
                            mappedItem.FazendaID = talhaoRelacionado.FazendaID;
                            mappedItem.ClienteID = talhaoRelacionado.ClienteID;
                        }
                    }
                    catch
                    {
                        // ignore failures here - retornaremos sem os IDs quando não disponíveis
                    }
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

        public async Task<List<object>> ListarRelatorioColetasAsync(Guid userID)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapaMobile(userID);
            if (visualizarMapa == null || visualizarMapa.Count == 0)
            {
                return [];
            }

            var result = new List<object>();

            foreach (var coleta in visualizarMapa)
            {
                if (coleta == null) continue;

                // Buscar dados do talhão
                var talhaoJson = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhaoJson == null) continue;

                // Buscar dados do funcionário responsável
                var funcionario = _usuarioService.BuscarUsuarioPorId(coleta.UsuarioRespID);

                // Buscar dados do GeoJSON para contar pontos planejados
                var geojson = _geoJsonRepository.ObterPorId(coleta.GeojsonID);
                int totalPontosPlanejados = ContarPontosPlanejados(geojson);

                // Buscar estatísticas de pontos coletados
                var pontosColetados = await _pontoColetadoRepository.BuscarPontosPorColetaAsync(coleta.Id);
                var ultimoPontoColetado = await _pontoColetadoRepository.BuscarUltimoPontoColetadoAsync(coleta.Id);
                var dataUltimaColeta = await _pontoColetadoRepository.ObterDataUltimaColetaAsync(coleta.Id);

                // Calcular estatísticas
                int totalPontosColetados = pontosColetados.Count;
                int pontosRestantes = totalPontosPlanejados - totalPontosColetados;
                decimal percentualConcluido = totalPontosPlanejados > 0
                    ? Math.Round((decimal)totalPontosColetados / totalPontosPlanejados * 100, 2)
                    : 0;

                // Determinar status da coleta
                string statusColeta = DeterminarStatusColeta(totalPontosColetados, totalPontosPlanejados);

                // Montar objeto do relatório
                var relatorioItem = new
                {
                    id = coleta.Id,
                    nomeColeta = talhaoJson.Nome ?? "Coleta sem nome",
                    talhao = new
                    {
                        id = talhaoJson.Id,
                        nome = talhaoJson.Nome,
                        area = talhaoJson.Area,
                        observacao = talhaoJson.Observacao
                    },
                    funcionarioResponsavel = funcionario != null ? new
                    {
                        nomeCompleto = funcionario.NomeCompleto,
                        cpf = funcionario.CPF,
                        email = funcionario.Email,
                        telefone = funcionario.Telefone
                    } : null,
                    estatisticas = new
                    {
                        totalPontosPlanejados = totalPontosPlanejados,
                        totalPontosColetados = totalPontosColetados,
                        pontosRestantes = pontosRestantes,
                        percentualConcluido = percentualConcluido,
                        statusColeta = statusColeta
                    },
                    ultimaColeta = ultimoPontoColetado != null ? new
                    {
                        dataColeta = ultimoPontoColetado.DataColeta,
                        latitude = ultimoPontoColetado.Latitude,
                        longitude = ultimoPontoColetado.Longitude,
                        funcionario = _usuarioService.BuscarUsuarioPorId(ultimoPontoColetado.FuncionarioID)?.NomeCompleto ?? "Funcionário não encontrado"
                    } : null,
                    dataUltimaColeta = dataUltimaColeta,
                    tipoColeta = coleta.TipoColeta.ToString(),
                    profundidade = coleta.Profundidade.ToString(),
                    observacao = coleta.Observacao ?? "",
                    dataCriacao = coleta.DataInclusao,
                    geojson = geojson != null ? new
                    {
                        id = geojson.Id,
                        pontos = geojson.Pontos,
                        grid = geojson.Grid
                    } : null
                };

                result.Add(relatorioItem);
            }

            return result;
        }

        private int ContarPontosPlanejados(Geojson? geojson)
        {
            if (geojson == null || string.IsNullOrEmpty(geojson.Pontos))
                return 0;

            try
            {
                var pontosJson = JsonSerializer.Deserialize<JsonElement>(geojson.Pontos);

                if (pontosJson.TryGetProperty("points", out JsonElement pointsElement))
                {
                    return pointsElement.GetArrayLength();
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private string DeterminarStatusColeta(int pontosColetados, int totalPontos)
        {
            if (pontosColetados == 0)
                return "Pendente";
            else if (pontosColetados < totalPontos)
                return "Em Andamento";
            else
                return "Finalizada";
        }

        public bool? SalvarColeta(Guid id, ColetaMobileDTO coleta)
        {
            try
            {
                // Busca a coleta pelo ID
                Coleta? co = _visualizarMapaRepository.ObterVisualizarMapaPorId(coleta.ColetaID, id);
                if (co == null)
                {
                    return false;
                }

                coleta.FuncionarioID = id;

                // Busca o GeoJSON associado à coleta
                Geojson? geo = _geoJsonRepository.ObterPorId(co.GeojsonID);
                if (geo == null || string.IsNullOrEmpty(geo.Pontos))
                {
                    return false;
                }

                // Deserializa o GeoJSON usando as opções estáticas
                var geoJson = JsonSerializer.Deserialize<JsonElement>(geo.Pontos, _jsonOptions);

                // Verifica se o GeoJSON tem a estrutura esperada (type, features, points)
                if (!geoJson.TryGetProperty("type", out var typeElement) ||
                    !geoJson.TryGetProperty("features", out var featuresElement) ||
                    !geoJson.TryGetProperty("points", out var pointsElement))
                {
                    return false;
                }

                // Cria um documento JSON para manipulação
                using var jsonDoc = JsonDocument.Parse(geo.Pontos);
                using var outputMs = new MemoryStream();
                using var writer = new Utf8JsonWriter(outputMs, new JsonWriterOptions { Indented = true });

                writer.WriteStartObject();

                // Escreve o tipo (FeatureCollection)
                writer.WriteString("type", typeElement.GetString());

                // Copia as features sem alteração
                writer.WritePropertyName("features");
                featuresElement.WriteTo(writer);

                // Processa os pontos, alterando apenas o status 'coletado' do ponto específico
                writer.WritePropertyName("points");
                writer.WriteStartArray();

                foreach (var point in pointsElement.EnumerateArray())
                {
                    // Verifica se é o ponto que queremos atualizar
                    if (point.TryGetProperty("properties", out var props) &&
                        props.TryGetProperty("id", out var idElement) &&
                        idElement.GetInt32() == coleta.Ponto.Id)
                    {
                        // Este é o ponto que queremos atualizar - cria uma cópia com 'coletado' = true
                        writer.WriteStartObject();

                        // Copia todas as propriedades do ponto
                        foreach (var property in point.EnumerateObject())
                        {
                            if (property.Name == "properties")
                            {
                                writer.WritePropertyName("properties");
                                writer.WriteStartObject();

                                // Copia todas as propriedades, alterando apenas 'coletado' para true
                                foreach (var prop in property.Value.EnumerateObject())
                                {
                                    if (prop.Name == "coletado")
                                    {
                                        writer.WriteBoolean("coletado", true);
                                    }
                                    else
                                    {
                                        prop.WriteTo(writer);
                                    }
                                }

                                writer.WriteEndObject(); // Fecha properties
                            }
                            else
                            {
                                // Copia outras propriedades do ponto (type, geometry, etc.)
                                property.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject(); // Fecha o ponto
                    }
                    else
                    {
                        // Não é o ponto que queremos atualizar, copia sem alteração
                        point.WriteTo(writer);
                    }
                }

                writer.WriteEndArray(); // Fecha o array de pontos
                writer.WriteEndObject(); // Fecha o objeto GeoJSON
                writer.Flush();

                // Converte o resultado para string e atualiza o GeoJSON
                outputMs.Position = 0;
                geo.Pontos = Encoding.UTF8.GetString(outputMs.ToArray());

                // Salva as alterações no GeoJSON
                _geoJsonRepository.Atualizar(geo);

                // Salva o ponto coletado na nova tabela
                var pontoColetado = new PontoColetado
                {
                    ColetaID = coleta.ColetaID,
                    FuncionarioID = id,
                    PontoID = Guid.NewGuid(),
                    HexagonID = Guid.NewGuid(),
                    Latitude = coleta.Latitude,
                    Longitude = coleta.Longitude,
                    DataColeta = coleta.DataColeta != default ? coleta.DataColeta : DateTime.Now
                };

                _pontoColetadoRepository.Adicionar(pontoColetado);
                UnitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Log do erro
                Console.WriteLine($"Erro ao atualizar o status de coleta: {ex.Message}");
                return false;
            }
        }
    }
}
