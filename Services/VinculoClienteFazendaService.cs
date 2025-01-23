using api.coleta.Data.Repository;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.vinculoClienteFazenda.Models.DTOs;
using AutoMapper;

namespace api.vinculoClienteFazenda.Services
{
   public class VinculoClienteFazendaService : ServiceBase
   {
      private readonly VinculoClienteFazendaRepository _vinculoRepository;

      public VinculoClienteFazendaService(VinculoClienteFazendaRepository vinculoRepository, IUnitOfWork unitOfWork, IMapper mapper)
          : base(unitOfWork, mapper)
      {
         _vinculoRepository = vinculoRepository;
      }

      public VinculoResponseDTO? BuscarVinculoPorId(Guid id)
      {
         var vinculo = _vinculoRepository.ObterPorId(id);
         if (vinculo == null)
         {
            return null;
         }
         return _mapper.Map<VinculoResponseDTO>(vinculo);
      }

      public void SalvarVinculo(VinculoRequestDTO vinculoDto)
      {
         var vinculoEntidade = _mapper.Map<VinculoClienteFazenda>(vinculoDto);
         _vinculoRepository.Adicionar(vinculoEntidade);
         UnitOfWork.Commit();
      }

      public void AtualizarVinculo(VinculoRequestDTO vinculoDto)
      {
         var vinculoEntidade = _mapper.Map<VinculoClienteFazenda>(vinculoDto);
         _vinculoRepository.Atualizar(vinculoEntidade);
         UnitOfWork.Commit();
      }

      public void DeletarVinculo(Guid id)
      {
         var vinculo = _vinculoRepository.ObterPorId(id);
         _vinculoRepository.Deletar(vinculo);
         UnitOfWork.Commit();
      }
   }
}
