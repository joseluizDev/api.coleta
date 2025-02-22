using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;
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

        public Cliente? BuscarClienteId(Guid idUser, Guid id)
        {
            return Context.Clientes.FirstOrDefault(c => c.Id == id && c.UsuarioID == idUser);
        }

        public void AtualizarCliente(Cliente cliente)
        {
            Atualizar(cliente);
        }

        public void DeletarCliente(Cliente cliente)
        {
            Deletar(cliente);
        }

        public List<Cliente> BuscarClientes(Guid id, int page)
        {

            List<Cliente> clientes = Context.Clientes
                .OrderBy(f => f.Id)
                .Skip((page - 1) * 10)
                .Take(10)
                .Where(c => c.UsuarioID == id)
                .ToList();

            return clientes;
        }

        public PagedResult<Cliente> listarClientes(Guid id, int page)
        {
            if (page < 1) page = 1;

            int totalItems = Context.Clientes.Count();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Cliente> clientes = Context.Clientes
                .Where(c => c.UsuarioID == id)
                .OrderBy(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Cliente>
            {
                Items = clientes,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }



        public List<Cliente> ListarTodosClientes(Guid userId)
        {
            return Context.Clientes.Where(c => c.UsuarioID == userId).ToList();
        }
    }

}
