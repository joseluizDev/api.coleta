using api.coleta.Data.Repositories;
using api.coleta.models.dtos;
using api.coleta.Models.Entidades;
using api.coleta.Utils;
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

        public async Task<PagedResult<Relatorio>> ListarRelatoriosPorUploadAsync(Guid userId, QueryRelatorio query)
        {
            if (query.Page < 1)
                query.Page = 1;

            int pageSize = query.PageSize > 0 ? query.PageSize : 10;
            int page = query.Page;

            var relatoriosQuery = Context.Relatorios
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Fazenda)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Cliente)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Safra!)
                        .ThenInclude(s => s.Fazenda)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.UsuarioResp)
                .Where(x => x.UsuarioId == userId && !string.IsNullOrEmpty(x.LinkBackup));

            // Filtro por Safra
            if (query.SafraID.HasValue)
                relatoriosQuery = relatoriosQuery.Where(x => x.Coleta != null && x.Coleta.SafraID == query.SafraID.Value);

            // Filtro por Cliente (através do Talhao)
            if (query.ClienteID.HasValue)
                relatoriosQuery = relatoriosQuery.Where(x => x.Coleta != null && x.Coleta.Talhao != null && x.Coleta.Talhao.Talhao != null && x.Coleta.Talhao.Talhao.ClienteID == query.ClienteID.Value);

            // Filtro por Fazenda
            if (query.FazendaID.HasValue)
                relatoriosQuery = relatoriosQuery.Where(x => x.Coleta != null && x.Coleta.FazendaID == query.FazendaID.Value);

            // Filtro por Talhão
            if (query.TalhaoID.HasValue)
                relatoriosQuery = relatoriosQuery.Where(x => x.Coleta != null && x.Coleta.Talhao != null && x.Coleta.Talhao.Id == query.TalhaoID.Value);

            // Contagem total
            int totalItems = await relatoriosQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Listagem com paginação
            List<Relatorio> relatorios = await relatoriosQuery
                .OrderByDescending(x => x.DataInclusao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Relatorio>
            {
                Items = relatorios,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public Task<Relatorio?> ObterPorId(Guid id, Guid userId)
        {
            return Context.Relatorios
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Fazenda)
                                .ThenInclude(f => f.Cliente)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Safra!)
                        .ThenInclude(s => s.Fazenda)
                            .ThenInclude(f => f.Cliente)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.UsuarioResp)
                .FirstOrDefaultAsync(x => x.ColetaId == id && x.UsuarioId == userId);
        }

        public Task<Relatorio?> ObterPorIdColetaRelatorio(Guid coletaId, Guid relatorioId, Guid userId)
        {
            return Context.Relatorios
                .FirstOrDefaultAsync(x => x.ColetaId == coletaId && x.Id == relatorioId && x.UsuarioId == userId);
        }

        public Task<Relatorio?> ObterPorRelatorioId(Guid relatorioId, Guid userId)
        {
            return Context.Relatorios
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Fazenda)
                                .ThenInclude(f => f.Cliente)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.Safra!)
                        .ThenInclude(s => s.Fazenda)
                            .ThenInclude(f => f.Cliente)
                .Include(x => x.Coleta!)
                    .ThenInclude(c => c.UsuarioResp)
                .FirstOrDefaultAsync(x => x.Id == relatorioId && x.UsuarioId == userId);
        }
    }
}
