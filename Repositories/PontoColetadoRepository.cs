using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class PontoColetadoRepository : GenericRepository<PontoColetado>
    {
        public PontoColetadoRepository(ApplicationDbContext context) : base(context) { }

        public void Adicionar(PontoColetado entity)
        {
            Context.Add(entity);
        }

        public void Atualizar(PontoColetado entity)
        {
            Context.Update(entity);
        }

        public void Deletar(PontoColetado entity)
        {
            Context.Remove(entity);
        }

        public async Task<List<PontoColetado>> BuscarPontosPorColetaAsync(Guid coletaId)
        {
            return await Context.PontoColetados
                .Where(p => p.ColetaID == coletaId)
                .OrderBy(p => p.DataColeta)
                .ToListAsync();
        }

        public async Task<PontoColetado?> BuscarUltimoPontoColetadoAsync(Guid coletaId)
        {
            return await Context.PontoColetados
                .Where(p => p.ColetaID == coletaId)
                .OrderByDescending(p => p.DataColeta)
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime?> ObterDataPrimeiraColetaAsync(Guid coletaId)
        {
            var primeiroPonto = await Context.PontoColetados
                .Where(p => p.ColetaID == coletaId)
                .OrderBy(p => p.DataColeta)
                .FirstOrDefaultAsync();

            return primeiroPonto?.DataColeta;
        }

        public async Task<DateTime?> ObterDataUltimaColetaAsync(Guid coletaId)
        {
            var ultimoPonto = await Context.PontoColetados
                .Where(p => p.ColetaID == coletaId)
                .OrderByDescending(p => p.DataColeta)
                .FirstOrDefaultAsync();

            return ultimoPonto?.DataColeta;
        }

        public async Task<int> ContarPontosColetadosAsync(Guid coletaId)
        {
            return await Context.PontoColetados
                .Where(p => p.ColetaID == coletaId)
                .CountAsync();
        }

        public async Task<List<PontoColetado>> BuscarPontosPorFuncionarioAsync(Guid funcionarioId)
        {
            return await Context.PontoColetados
                .Where(p => p.FuncionarioID == funcionarioId)
                .OrderByDescending(p => p.DataColeta)
                .ToListAsync();
        }

        public async Task<List<PontoColetado>> BuscarPontosPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await Context.PontoColetados
                .Where(p => p.DataColeta >= dataInicio && p.DataColeta <= dataFim)
                .OrderBy(p => p.DataColeta)
                .ToListAsync();
        }
    }
}
