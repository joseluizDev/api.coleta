﻿using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils;
using AutoMapper;

namespace api.coleta.Services
{
    public class VisualizarMapaService : ServiceBase
    {
        private readonly VisualizarMapaRepository _visualizarMapaRepository;
        public VisualizarMapaService(VisualizarMapaRepository visualizarMapaRepository, IUnitOfWork unitOfWork, IMapper mapper)
            : base(unitOfWork, mapper)
        {
            _visualizarMapaRepository = visualizarMapaRepository;
        }

        public VisualizarMapOutputDto? Salvar(Guid userID, VisualizarMapInputDto visualizarMapa)
        {
            var map = _mapper.Map<VisualizarMapa>(visualizarMapa);
            map.UsuarioID = userID;
            _visualizarMapaRepository.SalvarVisualizarMapa(map);
            if (UnitOfWork.Commit())
            {
                return _mapper.Map<VisualizarMapOutputDto>(map);
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
