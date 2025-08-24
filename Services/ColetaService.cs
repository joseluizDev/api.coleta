using api.coleta.models;
using api.coleta.models.dtos;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Settings;
using api.coleta.Utils;
using AutoMapper;
using Microsoft.Extensions.Options;

namespace api.coleta.repositories
{
    public class ColetaService : ServiceBase
    {
        private readonly ColetaRepository _coletaRepository;

        public ColetaService(ColetaRepository coletaRepository, IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            _coletaRepository = coletaRepository;
        }

        public ColetaResponseDTO? BuscarColetaPorId(Guid id)
        {
            var coleta = _coletaRepository.ObterPorId(id);

            if (coleta == null)
            {
                return null;
            }
            return _mapper.Map<ColetaResponseDTO>(coleta);
        }

        public void SalvarColetas(ColetaRequestDTO coletas)
        {
            var coletaEntidade = _mapper.Map<MColeta>(coletas);
            _coletaRepository.Adicionar(coletaEntidade);
            UnitOfWork.Commit();
        }

        public void AtualizarColeta(ColetaRequestDTO coleta)
        {
            var coletaEntidade = _mapper.Map<MColeta>(coleta);
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

            var coletasDtos = _mapper.Map<List<ColetaPorUsuarioDto>>(coletas.Items);

            return new PagedResult<ColetaPorUsuarioDto>
            {
                Items = coletasDtos,
                TotalPages = coletas.TotalPages,
                CurrentPage = coletas.CurrentPage
            };
        }

    }
}