using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;

namespace api.funcionario.Repositories
{
   public class FuncionarioRepository : GenericRepository<Funcionario>
   {
      public FuncionarioRepository(ApplicationDbContext context) : base(context)
      { }

      public void SalvarFuncionarios(List<Funcionario> funcionarios)
      {
         foreach (var funcionario in funcionarios)
         {
            Adicionar(funcionario);
         }
      }

      public Funcionario BuscarFuncionarioPorId(Guid id)
      {
         return ObterPorId(id);
      }

      public void AtualizarFuncionario(Funcionario funcionario)
      {
         Atualizar(funcionario);
      }

      public void DeletarFuncionario(Funcionario funcionario)
      {
         Deletar(funcionario);
      }
   }
}
