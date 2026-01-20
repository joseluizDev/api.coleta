using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class ContatoRepository : GenericRepository<Contato>
    {
        public ContatoRepository(ApplicationDbContext context) : base(context) { }

        public void Adicionar(Contato entity)
        {
            Context.Add(entity);
        }

        public void Atualizar(Contato entity)
        {
            Context.Update(entity);
        }

        public List<Contato> ListarContatos(int page, int pageSize)
        {
            return Context.Contatos
                .OrderByDescending(c => c.DataInclusao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int ContarContatos()
        {
            return Context.Contatos.Count();
        }

        public Task<Contato?> ObterPorIdAsync(Guid id)
        {
            return Context.Contatos.FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
