using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;

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
   }
}
