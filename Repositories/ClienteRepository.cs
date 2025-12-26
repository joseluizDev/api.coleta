using api.cliente.Models.DTOs;
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

        public PagedResult<Cliente> ListarClientes(Guid id, QueryClienteDTO query)
        {
            if (query.Page is null || query.Page < 1)
                query.Page = 1;

            int pageSize = 10;
            int page = query.Page.Value;

            var clientesQuery = Context.Clientes
                .Where(c => c.UsuarioID == id);

            if (!string.IsNullOrWhiteSpace(query.Nome))
                clientesQuery = clientesQuery.Where(c => c.Nome.Contains(query.Nome));

            if (!string.IsNullOrWhiteSpace(query.Email))
                clientesQuery = clientesQuery.Where(c => c.Email.Contains(query.Email));

            if (!string.IsNullOrWhiteSpace(query.Telefone))
                clientesQuery = clientesQuery.Where(c => c.Telefone.Contains(query.Telefone));

            int totalItems = clientesQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Cliente> clientes = clientesQuery
                .OrderBy(c => c.Id)
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
