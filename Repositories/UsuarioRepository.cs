using api.coleta.Models.Entidades;
using api.coleta.Data.Repositories;
using api.coleta.Utils;
using api.fazenda.Models.Entidades;
using api.funcionario.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Data.Repository
{
    public class UsuarioRepository : GenericRepository<Usuario>
    {
        public UsuarioRepository(ApplicationDbContext context) : base(context)
        { }
        public Usuario? Login(string email, string senha)
        {
            return Context.Usuarios.FirstOrDefault(x => x.Email == email && x.Senha == senha);
        }
        public Usuario? ObterPorEmail(string email)
        {
            return Context.Usuarios.FirstOrDefault(x => x.Email == email);
        }
        public Usuario? ObterPorCpf(string cpf)
        {
            return Context.Usuarios.FirstOrDefault(x => x.CPF == cpf);
        }

        public List<Usuario> ListarUsuariosPorFuncionario(Guid id)
        {
            return Context.Usuarios
                .Where(x => x.adminId != null && x.adminId == id)
                .ToList();
        }
        public Usuario? LoginMobile(string email, string senha)
        {
            return Context.Usuarios.Where(x => x.Email == email && x.Senha == senha).FirstOrDefault();
        }

        public PagedResult<Usuario> ListarFuncionarios(QueryFuncionario query, Guid userId)
        {
            if (query.Page is null || query.Page < 1)
                query.Page = 1;

            int pageSize = 10;
            int page = query.Page.Value;

            var clientesQuery = Context.Usuarios
                .Where(c => c.adminId == userId);


            if (!string.IsNullOrWhiteSpace(query.Nome))
                clientesQuery = clientesQuery.Where(c => c.NomeCompleto.Contains(query.Nome));

            if (!string.IsNullOrWhiteSpace(query.CPF))
                clientesQuery = clientesQuery.Where(c => c.CPF.Contains(query.CPF));

            if (!string.IsNullOrWhiteSpace(query.Email))
                clientesQuery = clientesQuery.Where(c => c.Email.Contains(query.Email));

            if (!string.IsNullOrWhiteSpace(query.Telefone))
                clientesQuery = clientesQuery.Where(c => c.Telefone.Contains(query.Telefone));

            int totalItems = clientesQuery.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Usuario> usuarios = clientesQuery
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Usuario>
            {
                Items = usuarios,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
