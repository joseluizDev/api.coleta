using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class HistoricoPagamentoRepository : GenericRepository<HistoricoPagamento>
    {
        public HistoricoPagamentoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<HistoricoPagamento?> ObterPorIdAsync(Guid id)
        {
            return await Context.HistoricosPagamento
                .Include(p => p.Assinatura)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletadoEm == null);
        }

        public async Task<HistoricoPagamento?> ObterPorPixTxIdAsync(string pixTxId)
        {
            return await Context.HistoricosPagamento
                .Include(p => p.Assinatura)
                .FirstOrDefaultAsync(p => p.PixTxId == pixTxId && p.DeletadoEm == null);
        }

        public async Task<HistoricoPagamento?> ObterPorEfiPayChargeIdAsync(string chargeId)
        {
            return await Context.HistoricosPagamento
                .Include(p => p.Assinatura)
                .FirstOrDefaultAsync(p => p.EfiPayChargeId == chargeId && p.DeletadoEm == null);
        }

        public async Task<List<HistoricoPagamento>> ObterPagamentosDaAssinaturaAsync(Guid assinaturaId)
        {
            return await Context.HistoricosPagamento
                .Include(p => p.Assinatura)
                .Where(p => p.AssinaturaId == assinaturaId && p.DeletadoEm == null)
                .OrderByDescending(p => p.DataPagamento)
                .ToListAsync();
        }

        public async Task<List<HistoricoPagamento>> ObterPagamentosPendentesExpiradosAsync()
        {
            return await Context.HistoricosPagamento
                .Include(p => p.Assinatura)
                .Where(p => p.Status == StatusPagamento.Pendente
                         && p.DataExpiracao.HasValue
                         && p.DataExpiracao.Value < DateTime.Now
                         && p.DeletadoEm == null)
                .ToListAsync();
        }

        public async Task<decimal> ObterTotalPagoDoClienteAsync(Guid clienteId)
        {
            return await Context.HistoricosPagamento
                .Include(p => p.Assinatura)
                .Where(p => p.Assinatura.ClienteId == clienteId
                         && p.Status == StatusPagamento.Aprovado
                         && p.DeletadoEm == null)
                .SumAsync(p => p.Valor);
        }

        public void Atualizar(HistoricoPagamento pagamento)
        {
            Context.HistoricosPagamento.Update(pagamento);
        }
    }
}
