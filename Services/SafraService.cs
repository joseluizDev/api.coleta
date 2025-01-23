using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.safra.Models.DTOs;
using api.safra.Repositories;
using AutoMapper;

namespace api.safra.Services
{
   public class SafraService : ServiceBase
   {
      private readonly SafraRepository _safraRepository;

      public SafraService(SafraRepository safraRepository, IUnitOfWork unitOfWork, IMapper mapper)
          : base(unitOfWork, mapper)
      {
         _safraRepository = safraRepository;
      }

      public SafraResponseDTO? BuscarSafraPorId(Guid id)
      {
         var safra = _safraRepository.ObterPorId(id);
         if (safra == null)
         {
            return null;
         }
         return _mapper.Map<SafraResponseDTO>(safra);
      }

      public void SalvarSafra(SafraRequestDTO safraDto)
      {
         var safraEntidade = _mapper.Map<Safra>(safraDto);
         _safraRepository.Adicionar(safraEntidade);
         UnitOfWork.Commit();
      }

      public void AtualizarSafra(SafraRequestDTO safraDto)
      {
         var safraEntidade = _mapper.Map<Safra>(safraDto);
         _safraRepository.Atualizar(safraEntidade);
         UnitOfWork.Commit();
      }

      public void DeletarSafra(Guid id)
      {
         var safra = _safraRepository.ObterPorId(id);
         _safraRepository.Deletar(safra);
         UnitOfWork.Commit();
      }
   }
}
