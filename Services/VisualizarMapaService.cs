using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using AutoMapper;

namespace api.coleta.Services
{
    public class VisualizarMapaService : ServiceBase
    {
        private readonly VisualizarMapaRepository _visualizarMapaRepository;
        private readonly GeoJsonRepository _geoJsonRepository;
        public VisualizarMapaService(VisualizarMapaRepository visualizarMapaRepository, IUnitOfWork unitOfWork, IMapper mapper, GeoJsonRepository geoJsonRepository)
            : base(unitOfWork, mapper)
        {
            _visualizarMapaRepository = visualizarMapaRepository;
            _geoJsonRepository = geoJsonRepository;
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
                return new PagedResult<VisualizarMapOutputDto?>
                {
                    Items = mappedItems,
                    TotalPages = visualizarMapa.TotalPages,
                    CurrentPage = visualizarMapa.CurrentPage
                };
            }
            return null;
        }
    }
}
