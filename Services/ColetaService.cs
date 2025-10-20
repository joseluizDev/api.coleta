using System;
using api.coleta.models;
using api.coleta.models.dtos;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Settings;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.coleta.Interfaces;
using api.coleta.Data.Repository;
using Microsoft.Extensions.Options;

namespace api.coleta.repositories
{
    public class ColetaService : ServiceBase
    {
        private readonly ColetaRepository _coletaRepository;
        private readonly UsuarioRepository _usuarioRepository;
        private readonly IOneSignalService _oneSignalService;

        public ColetaService(ColetaRepository coletaRepository, UsuarioRepository usuarioRepository, IOneSignalService oneSignalService, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _coletaRepository = coletaRepository;
            _usuarioRepository = usuarioRepository;
            _oneSignalService = oneSignalService;
        }

        public ColetaResponseDTO? BuscarColetaPorId(Guid id)
        {
            var coleta = _coletaRepository.ObterPorId(id);

            if (coleta == null)
            {
                return null;
            }
            return coleta.ToResponseDto();
        }

        public async Task SalvarColetas(ColetaRequestDTO coletas, Guid usuarioId)
        {
            var coletaEntidade = coletas.ToEntity();
            if (coletaEntidade == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados da coleta.");
            }
            _coletaRepository.Adicionar(coletaEntidade);
            UnitOfWork.Commit();
        }

        public void AtualizarColeta(ColetaRequestDTO coleta)
        {
            var coletaEntidade = coleta.ToEntity();
            if (coletaEntidade == null)
            {
                throw new InvalidOperationException("Não foi possível converter os dados da coleta.");
            }
            _coletaRepository.Atualizar(coletaEntidade);
            UnitOfWork.Commit();
        }

        public void DeletarColeta(Guid id)

        {
            var coleta = _coletaRepository.ObterPorId(id);

            _coletaRepository.Deletar(coleta);
            UnitOfWork.Commit();
        }

        public async Task<PagedResult<ColetaPorUsuarioDto>> BucarColetasPorUsuarioAsync(Guid userID, QueryColeta query)
        {
            // Await no método assíncrono
            var coletas = await _coletaRepository.BuscarColetasPorUsuario(userID, query);

            var coletasDtos = coletas.Items.ToUsuarioDtoList();

            return new PagedResult<ColetaPorUsuarioDto>
            {
                Items = coletasDtos,
                TotalPages = coletas.TotalPages,
                CurrentPage = coletas.CurrentPage
            };
        }

        public MColeta? ObterColetaPorId(Guid id)
        {
            return _coletaRepository.ObterPorId(id);
        }
    }
}
