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

        public Task<List<Relatorio>> ListarRelatoriosPorUploadAsync(Guid userId)
        {
            return Context.Relatorios
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Fazenda)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Safra!)
                        .ThenInclude(s => s.Fazenda)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.UsuarioResp)
                .Where(x => x.UsuarioId == userId && !string.IsNullOrEmpty(x.LinkBackup))
                .ToListAsync();
        }

        public Task<Relatorio?> ObterPorId(Guid id, Guid userId)
        {
            return Context.Relatorios
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Fazenda)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Safra!)
                        .ThenInclude(s => s.Fazenda)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.UsuarioResp)
                .FirstOrDefaultAsync(x => x.ColetaId == id && x.UsuarioId == userId);
        }

        public Task<Relatorio?> ObterPorIdColetaRelatorio(Guid coletaId, Guid relatorioId, Guid userId)
        {
            return Context.Relatorios
                .FirstOrDefaultAsync(x => x.ColetaId == coletaId && x.Id == relatorioId && x.UsuarioId == userId);
        }
    }
}
