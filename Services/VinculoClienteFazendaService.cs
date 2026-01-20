using api.coleta.Data.Repository;
using System;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.coleta.Utils.Maps;
using api.vinculoClienteFazenda.Models.DTOs;

namespace api.vinculoClienteFazenda.Services
{
   public class VinculoClienteFazendaService : ServiceBase
   {
      private readonly VinculoClienteFazendaRepository _vinculoRepository;

      public VinculoClienteFazendaService(VinculoClienteFazendaRepository vinculoRepository, IUnitOfWork unitOfWork)
          : base(unitOfWork)
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
         return vinculo.ToResponseDto();
      }

      public void SalvarVinculo(VinculoRequestDTO vinculoDto)
      {
         var vinculoEntidade = vinculoDto.ToEntity();
         if (vinculoEntidade == null)
         {
            throw new InvalidOperationException("Não foi possível converter os dados do vínculo.");
         }
         _vinculoRepository.Adicionar(vinculoEntidade);
         UnitOfWork.Commit();
      }

      public void AtualizarVinculo(VinculoRequestDTO vinculoDto)
      {
         var vinculoEntidade = vinculoDto.ToEntity();
         if (vinculoEntidade == null)
         {
            throw new InvalidOperationException("Não foi possível converter os dados do vínculo.");
         }
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
