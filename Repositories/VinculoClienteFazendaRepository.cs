


using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;

namespace api.coleta.Data.Repository
{
   public class VinculoClienteFazendaRepository : GenericRepository<VinculoClienteFazenda>
   {
      public VinculoClienteFazendaRepository(ApplicationDbContext context) : base(context)
      { }

      public void SalvarVinculos(List<VinculoClienteFazenda> vinculos)
      {
         foreach (var vinculo in vinculos)
         {
            Adicionar(vinculo);
         }
      }

      public VinculoClienteFazenda BuscarVinculoPorId(Guid id)
      {
         return ObterPorId(id);
      }

      public void AtualizarVinculo(VinculoClienteFazenda vinculo)
      {
         Atualizar(vinculo);
      }

      public void DeletarVinculo(VinculoClienteFazenda vinculo)
      {
         Deletar(vinculo);
      }
   }
}
