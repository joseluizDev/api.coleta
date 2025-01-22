using api.coleta.Services;
using api.coleta.Settings;
using api.fazenda.models;
using api.fazenda.Models.Entidades;
using AutoMapper;
using Microsoft.Extensions.Options;


namespace api.fazenda.repositories
{
   public class FazendaService : ServiceBase
   {
      private readonly FazendaRepository _fazendaRepository;

      public FazendaService(FazendaRepository fazendaRepository, IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
      {
         _fazendaRepository = fazendaRepository;
      }

      public FazendaResponseDTO? BuscarFazendaPorId(Guid id)
      {
         var fazenda = _fazendaRepository.ObterPorId(id);

         if (fazenda == null)
         {
            return null;
         }

         return _mapper.Map<FazendaResponseDTO>(fazenda);
      }

      public void SalvarFazendas(FazendaRequestDTO fazendas)
      {
         var fazendaEntidade = _mapper.Map<Fazenda>(fazendas);
         _fazendaRepository.Adicionar(fazendaEntidade);
         UnitOfWork.Commit();
      }

      public void AtualizarFazenda(FazendaRequestDTO fazenda)
      {
         var fazendaEntidade = _mapper.Map<Fazenda>(fazenda);
         _fazendaRepository.Atualizar(fazendaEntidade);
         UnitOfWork.Commit();
      }

      public void DeletarFazenda(Guid id)
      {
         var fazenda = _fazendaRepository.ObterPorId(id);
         _fazendaRepository.Deletar(fazenda);
         UnitOfWork.Commit();
      }
   }
}