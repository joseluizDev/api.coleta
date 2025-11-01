using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Models.DTOs;
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

        public async Task<(List<Relatorio> Items, int TotalItems)> ListarRelatoriosMobileAsync(
            Guid userId,
            QueryRelatorioMobile query)
        {
            // Validação de paginação
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1 || query.Limit > 100) query.Limit = 10;

            // Query base com todos os relacionamentos necessários
            var queryable = Context.Relatorios
                .Include(r => r.Coleta!)
                    .ThenInclude(c => c.Talhao!)
                        .ThenInclude(t => t.Talhao!)
                            .ThenInclude(tt => tt.Fazenda)
                .Include(r => r.Coleta!)
                    .ThenInclude(c => c.Geojson)
                .Include(r => r.Coleta!)
                    .ThenInclude(c => c.Safra!)
                        .ThenInclude(s => s.Fazenda)
                .Where(r => r.UsuarioId == userId);

            // Filtro por fazenda (case-insensitive)
            if (!string.IsNullOrWhiteSpace(query.Fazenda))
            {
                var fazendaLower = query.Fazenda.ToLower();
                queryable = queryable.Where(r =>
                    (r.Coleta!.Talhao!.Talhao!.Fazenda!.Nome.ToLower().Contains(fazendaLower)) ||
                    (r.Coleta!.Safra != null && r.Coleta.Safra.Fazenda!.Nome.ToLower().Contains(fazendaLower))
                );
            }

            // Filtro por talhão (case-insensitive)
            if (!string.IsNullOrWhiteSpace(query.Talhao))
            {
                var talhaoLower = query.Talhao.ToLower();
                queryable = queryable.Where(r =>
                    r.Coleta!.Talhao!.Nome.ToLower().Contains(talhaoLower)
                );
            }

            // Filtro por data inicial
            if (query.DataInicio.HasValue)
            {
                queryable = queryable.Where(r => r.DataInclusao.Date >= query.DataInicio.Value.Date);
            }

            // Filtro por data final
            if (query.DataFim.HasValue)
            {
                queryable = queryable.Where(r => r.DataInclusao.Date <= query.DataFim.Value.Date);
            }

            // Contar total de itens
            int totalItems = await queryable.CountAsync();

            // Aplicar paginação e ordenação (mais recentes primeiro)
            var items = await queryable
                .OrderByDescending(r => r.DataInclusao)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            return (items, totalItems);
        }
    }
}
