using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class PlanoRepository : GenericRepository<Plano>
    {
        public PlanoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Plano>> ListarPlanosAtivosAsync()
        {
            return await Context.Planos
                .Where(p => p.Ativo)
                .OrderBy(p => p.ValorAnual)
                .ToListAsync();
        }

        public async Task<Plano?> ObterPorIdAsync(Guid id)
        {
            return await Context.Planos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Plano?> ObterPorNomeAsync(string nome)
        {
            return await Context.Planos
                .FirstOrDefaultAsync(p => p.Nome == nome && p.Ativo);
        }

        public async Task<Plano?> ObterPorEfiPayPlanIdAsync(string efiPayPlanId)
        {
            return await Context.Planos
                .FirstOrDefaultAsync(p => p.EfiPayPlanId == efiPayPlanId);
        }

        public async Task<bool> ExistePlanoComNomeAsync(string nome, Guid? excluirId = null)
        {
            var query = Context.Planos.Where(p => p.Nome == nome);

            if (excluirId.HasValue)
            {
                query = query.Where(p => p.Id != excluirId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
