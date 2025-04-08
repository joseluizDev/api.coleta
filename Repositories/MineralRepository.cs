using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class MineralRepository : GenericRepository<Minerais>
    {
        public MineralRepository(ApplicationDbContext context) : base(context) { }
        public void Adicionar(Minerais entity)
        {
            Context.Add(entity);
        }
        public void Atualizar(Minerais entity)
        {
            Context.Update(entity);
        }
        public void Deletar(Minerais entity)
        {
            Context.Remove(entity);
        }
        public Task<List<Minerais>> ListarMinerais()
        {
            return Context.Minerais.ToListAsync();
        }
    }
}