using api.fazenda.Models.Entidades;
using whtsapp.Data.Repositories;

namespace api.fazenda.repositories
{
   public class FazendaRepository : GenericRepository<Fazenda>
   {
      public FazendaRepository(ApplicationDbContext context) : base(context)
      { }

      public void SalvarFazendas(List<Fazenda> fazendas)
      {
         foreach (var fazenda in fazendas)
         {
            Adicionar(fazenda);
         }
      }

      public Fazenda BuscarFazendaPorId(Guid id)
      {
         return ObterPorId(id);
      }

      public void AtualizarFazenda(Fazenda fazenda)
      {
         Atualizar(fazenda);
      }

      public void DeletarFazenda(Fazenda fazenda)
      {
         Deletar(fazenda);
      }
   }
}