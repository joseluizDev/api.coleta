using api.coleta.Models.Entidades;
using api.coleta.Data.Repositories;

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
    }
}
