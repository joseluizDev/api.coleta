using api.coleta.Models.Entidades;
using api.coleta.Data.Repositories;
using api.coleta.Utils;
using api.fazenda.Models.Entidades;

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
            return Context.Usuarios.Where(x => x.Email == email && x.Senha == senha && x.adminId != null).FirstOrDefault();
        }

        public PagedResult<Usuario> ListarFuncionarios(int page, Guid userId)
        {
            if (page < 1) page = 1;

            int totalItems = Context.Usuarios.Count();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            List<Usuario> usuarios = Context.Usuarios
                .OrderBy(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Where(f => f.adminId == userId)
                .ToList();

            return new PagedResult<Usuario>
            {
                Items = usuarios,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

    }
}
