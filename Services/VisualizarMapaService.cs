using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.funcionario.Models.DTOs;
using api.talhao.Models.DTOs;
using api.talhao.Repositories;
using api.talhao.Services;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using api.cliente.Models.DTOs;
using api.fazenda.models;
using api.fazenda.repositories;
using System.Text.Json;

namespace api.coleta.Services
{
    public class VisualizarMapaService : ServiceBase
    {
        private readonly VisualizarMapaRepository _visualizarMapaRepository;
        private readonly TalhaoRepository _talhaoRepository;
        private readonly GeoJsonRepository _geoJsonRepository;
        private readonly UsuarioService _usuarioService;
        private readonly TalhaoService _talhaoService;
        private readonly FazendaRepository _fazendaRepository;
        public VisualizarMapaService(
            UsuarioService usuarioService,
            VisualizarMapaRepository visualizarMapaRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            GeoJsonRepository geoJsonRepository,
            TalhaoRepository talhaoRepository,
            TalhaoService talhaoService,
            FazendaRepository fazendaRepository)
            : base(unitOfWork, mapper)
        {
            _visualizarMapaRepository = visualizarMapaRepository;
            _geoJsonRepository = geoJsonRepository;
            _talhaoRepository = talhaoRepository;
            _talhaoService = talhaoService;
            _usuarioService = usuarioService;
            _fazendaRepository = fazendaRepository;
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

        public List<ColetasPorFazendaDto> ListarMobilePorFazenda(Guid userID)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapaMobile(userID);
            if (visualizarMapa == null || visualizarMapa.Count == 0)
            {
                return [];
            }

            var mappedItems = _mapper.Map<List<VisualizarMapOutputDto?>>(visualizarMapa);
            var coletasPorFazenda = new Dictionary<Guid, ColetasPorFazendaDto>();

            foreach (var coleta in mappedItems)
            {
                if (coleta == null) continue;

                var talhaoJson = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                if (talhaoJson != null)
                {
                    coleta.Talhao = _mapper.Map<Talhoes>(talhaoJson);

                    // Buscar o talhão para obter a fazenda e o cliente
                    var talhaoBase = _talhaoService.BuscarTalhaoPorTalhaoJson(talhaoJson.Id);
                    if (talhaoBase != null && talhaoBase.FazendaID != Guid.Empty)
                    {
                        var fazendaId = talhaoBase.FazendaID;

                        // Adicionar a coleta ao grupo da fazenda
                        if (!coletasPorFazenda.TryGetValue(fazendaId, out var coletasPorFazendaDto))
                        {
                            var fazenda = talhaoBase.Fazenda;
                            var cliente = talhaoBase.Cliente;

                            // Se a fazenda não estiver carregada, buscar diretamente do repositório
                            if (fazenda == null)
                            {
                                var fazendaRepo = _fazendaRepository.BuscarFazendaPorId(fazendaId);
                                fazenda = fazendaRepo;
                            }

                            // Verificação adicional para garantir que temos o nome da fazenda
                            string nomeFazenda = "Fazenda sem nome";
                            if (fazenda != null && !string.IsNullOrEmpty(fazenda.Nome))
                            {
                                nomeFazenda = fazenda.Nome;
                            }

                            coletasPorFazendaDto = new ColetasPorFazendaDto
                            {
                                FazendaId = fazendaId,
                                NomeFazenda = nomeFazenda,
                                Fazenda = _mapper.Map<FazendaResponseDTO>(fazenda),
                                Cliente = _mapper.Map<ClienteResponseDTO>(cliente)
                            };
                            coletasPorFazenda[fazendaId] = coletasPorFazendaDto;
                        }

                        // Completar os dados da coleta
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

                        coletasPorFazendaDto.Coletas.Add(coleta);
                    }
                }
            }

            return [.. coletasPorFazenda.Values];
        }

        public bool? SalvarColeta(Guid id, ColetaMobileDTO coleta)
        {
            Coleta? co = _visualizarMapaRepository.ObterVisualizarMapaPorId(coleta.ColetaID, id);
            if (co != null)
            {
                coleta.FuncionarioID = id;
                Geojson geo = _geoJsonRepository.ObterPorId(co.GeojsonID);
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
                            if (ponto.Properties.Id == coleta.Ponto.Properties.Id)
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
