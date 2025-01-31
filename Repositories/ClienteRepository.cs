using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.cliente.Repositories
{
   public class ClienteRepository : GenericRepository<Cliente>
   {
      public ClienteRepository(ApplicationDbContext context) : base(context)
      { }

      public void SalvarClientes(List<Cliente> clientes)
      {
         foreach (var cliente in clientes)
         {
            Adicionar(cliente);
         }
      }

      public Cliente BuscarClientePorId(Guid id)
      {
         return ObterPorId(id);
      }

      public void AtualizarCliente(Cliente cliente)
      {
         Atualizar(cliente);
      }

      public void DeletarCliente(Cliente cliente)
      {
         Deletar(cliente);
      }

        public List<Cliente> BuscaPaginadaCliente(Guid id, int page = 1, int limit = 0)
        {

            IQueryable<Cliente> query = Context.Clientes.
                Where((cliente) => cliente.UsuarioID == id).
                OrderBy((cliente) => cliente.DataInclusao).AsQueryable();
            
            var clientes = BuscaPaginada(query, page, limit);
            
            return clientes;
        }
    }
}
