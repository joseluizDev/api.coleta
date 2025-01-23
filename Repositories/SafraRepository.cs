using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;

namespace api.safra.Repositories
{
   public class SafraRepository : GenericRepository<Safra>
   {
      public SafraRepository(ApplicationDbContext context) : base(context)
      { }

      public void SalvarSafras(List<Safra> safras)
      {
         foreach (var safra in safras)
         {
            Adicionar(safra);
         }
      }

      public Safra BuscarSafraPorId(Guid id)
      {
         return ObterPorId(id);
      }

      public void AtualizarSafra(Safra safra)
      {
         Atualizar(safra);
      }

      public void DeletarSafra(Safra safra)
      {
         Deletar(safra);
      }
   }
}
