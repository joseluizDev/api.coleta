using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;

namespace api.talhao.Repositories
{
   public class TalhaoRepository : GenericRepository<Talhao>
   {
      public TalhaoRepository(ApplicationDbContext context) : base(context)
      { }

      public void SalvarTalhoes(List<Talhao> talhoes)
      {
         foreach (var talhao in talhoes)
         {
            Adicionar(talhao);
         }
      }

      public Talhao BuscarTalhaoPorId(Guid id)
      {
         return ObterPorId(id);
      }

      public void AtualizarTalhao(Talhao talhao)
      {
         Atualizar(talhao);
      }

      public void DeletarTalhao(Talhao talhao)
      {
         Deletar(talhao);
      }
   }
}
