using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;

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
      
      public PagedResult<Funcionario> ListarFuncionarios(Guid userId, int page)
      {
         if (page < 1) page = 1;

         int totalItems = Context.Funcionarios.Count();
         int pageSize = 10;
         int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

         List<Funcionario> funcionarios = Context.Funcionarios
            .OrderBy(f => f.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Where(f => f.UsuarioID == userId)
            .ToList();

         return new PagedResult<Funcionario>
         {
            Items = funcionarios,
            TotalPages = totalPages,
            CurrentPage = page
         };
      }
   }
}
