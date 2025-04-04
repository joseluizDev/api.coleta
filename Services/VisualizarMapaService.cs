using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.funcionario.Models.DTOs;
using api.funcionario.Services;
using api.talhao.Models.DTOs;
using api.talhao.Repositories;
using api.talhao.Services;
using AutoMapper;

namespace api.coleta.Services
{
    public class VisualizarMapaService : ServiceBase
    {
        private readonly VisualizarMapaRepository _visualizarMapaRepository;
        private readonly TalhaoRepository _talhaoService;
        private readonly FuncionarioService _funcionarioService;
        private readonly GeoJsonRepository _geoJsonRepository;
        public VisualizarMapaService(FuncionarioService funcionarioService, VisualizarMapaRepository visualizarMapaRepository, IUnitOfWork unitOfWork, IMapper mapper, GeoJsonRepository geoJsonRepository, TalhaoRepository talhaoService)
            : base(unitOfWork, mapper)
        {
            _visualizarMapaRepository = visualizarMapaRepository;
            _geoJsonRepository = geoJsonRepository;
            _talhaoService = talhaoService;
            _funcionarioService = funcionarioService;
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

        public PagedResult<VisualizarMapOutputDto?> listar(Guid userID, int page)
        {
            var visualizarMapa = _visualizarMapaRepository.ListarVisualizarMapa(userID, page);
             if (visualizarMapa != null)
            {
                var mappedItems = _mapper.Map<List<VisualizarMapOutputDto?>>(visualizarMapa.Items);
                
                foreach(var coleta in mappedItems)
                {
                    var talhao = _talhaoService.BuscarTalhaoJsonPorId(coleta.TalhaoID);
                    if (talhao != null)
                    {
                        coleta.Talhao = _mapper.Map<Talhoes>(talhao);
                    }

                    var funcionario = _funcionarioService.BuscarFuncionarioPorId(coleta.FuncionarioID);
                    if (funcionario != null)
                    {
                        coleta.Funcionario = _mapper.Map<FuncionarioResponseDTO>(funcionario);
                    }
                }

                return new PagedResult<VisualizarMapOutputDto?>
                {
                    Items = mappedItems,
                    TotalPages = visualizarMapa.TotalPages,
                    CurrentPage = visualizarMapa.CurrentPage
                };
            }
            return null;
        }

        public bool? ExcluirColeta(Guid userID, Guid id)
        {
            VisualizarMapa buscarColeta = _visualizarMapaRepository.BuscarVisualizarMapaPorIdTalhao(userID, id);
            if (buscarColeta != null)
            {
                _visualizarMapaRepository.DeletarVisualizarMapa(buscarColeta);
                UnitOfWork.Commit();
                return true;
            }
            return false;
        }
    }
}
