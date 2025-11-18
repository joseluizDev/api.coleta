using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class RecomendacaoRepository : GenericRepository<Recomendacao>
    {
        public RecomendacaoRepository(ApplicationDbContext context) : base(context) { }
        
        public void Adicionar(Recomendacao entity)
        {
            Context.Add(entity);
        }
        
        public void Atualizar(Recomendacao entity)
        {
            Context.Update(entity);
        }
        
        public void Deletar(Recomendacao entity)
        {
            Context.Remove(entity);
        }
        
        public Task<List<Recomendacao>> ListarPorRelatorio(Guid relatorioId)
        {
            return Context.Recomendacoes
                .Where(x => x.RelatorioId == relatorioId)
                .OrderBy(x => x.DataInclusao)
                .ToListAsync();
        }
        
        public Task<List<Recomendacao>> ListarPorColeta(Guid coletaId)
        {
            return Context.Recomendacoes
                .Where(x => x.ColetaId == coletaId)
                .OrderBy(x => x.DataInclusao)
                .ToListAsync();
        }
        
        public Task<Recomendacao?> ObterPorId(Guid id)
        {
            return Context.Recomendacoes
                .Include(x => x.Relatorio)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Relatorio?> ObterRelatorioPorIdSeguro(Guid relatorioId, Guid userId)
        {
            return Context.Relatorios
                .FirstOrDefaultAsync(x => x.Id == relatorioId && x.UsuarioId == userId);
        }
    }
}
