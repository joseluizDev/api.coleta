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

        public List<Cliente> BuscarClientes(
            Guid id,
            int page = 1,
            int limit = 0,
            string searchTerm = ""
        )
        {

            var query = _FiltrarClientePorUsuario(id, searchTerm);

            var clientes = BuscaPaginada(query, page, limit);

            return clientes;
        }

        public int TotalClientes(Guid id, string searchTerm)
        {
            return _FiltrarClientePorUsuario(id, searchTerm).Count();
        }

        private IQueryable<Cliente> _FiltrarClientePorUsuario(Guid id, string searchTerm)
        {
            IQueryable<Cliente> query = Context.Clientes.
                Where((cliente) => cliente.UsuarioID == id);


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(
                    (cliente) => cliente.Nome.ToLower().Contains(searchTerm.ToLower())
                );
            }

            query = query.OrderBy((cliente) => cliente.DataInclusao).AsQueryable();
            return query;
        }
    }

}
