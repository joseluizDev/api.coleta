using api.coleta.Models.Entidades;
using whtsapp.Data.Repositories;

namespace whtsapp.Data.Repository
{
    public class UsuarioRepository : GenericRepository<Usuario>
    {
        public UsuarioRepository(ApplicationDbContext context) : base(context)
        { }
        public Usuario? Login(string login, string senha)
        {
            return Context.Usuarios.FirstOrDefault(x => x.Email == login && x.Senha == senha);
        }
        public Usuario? BuscarPorID(Guid id)
        {
            return Context.Usuarios.FirstOrDefault(x => x.Id == id);
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
