using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class RelatorioRepository : GenericRepository<Relatorio>
    {
        public RelatorioRepository(ApplicationDbContext context) : base(context) { }
        public void Adicionar(Relatorio entity)
        {
            Context.Add(entity);
        }
        public void Atualizar(Relatorio entity)
        {
            Context.Update(entity);
        }
        public void Deletar(Relatorio entity)
        {
            Context.Remove(entity);
        }
        public Task<List<Relatorio>> ListarRelatorios()
        {
            return Context.Relatorios.ToListAsync();
        }

        public Task<Relatorio?> ObterPorId(Guid id, Guid userId)
        {
            return Context.Relatorios.FirstOrDefaultAsync(x => x.ColetaId == id && x.UsuarioId == userId);
        }
    }
}
