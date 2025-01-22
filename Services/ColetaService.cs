using api.coleta.models;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Settings;
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
            var coletaEntidade = _mapper.Map<Coleta>(coletas);
            _coletaRepository.Adicionar(coletaEntidade);
            UnitOfWork.Commit();
        }

        public void AtualizarColeta(ColetaRequestDTO coleta)
        {
            var coletaEntidade = _mapper.Map<Coleta>(coleta);
            _coletaRepository.Atualizar(coletaEntidade);
            UnitOfWork.Commit();
        }

        public void DeletarColeta(Guid id)
        {
            var coleta = _coletaRepository.ObterPorId(id);
            _coletaRepository.Deletar(coleta);
            UnitOfWork.Commit();
        }
    }
}
