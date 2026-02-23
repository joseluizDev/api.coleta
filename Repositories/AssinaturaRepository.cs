using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class AssinaturaRepository : GenericRepository<Assinatura>
    {
        public AssinaturaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Assinatura?> ObterPorIdAsync(Guid id)
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletadoEm == null);
        }

        public async Task<Assinatura?> ObterAssinaturaAtivaDoClienteAsync(Guid clienteId)
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .Where(a => a.ClienteId == clienteId
                         && a.Ativa
                         && a.DataFim >= DateTime.Now
                         && a.DeletadoEm == null)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Assinatura>> ObterAssinaturasDoClienteAsync(Guid clienteId)
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .Where(a => a.ClienteId == clienteId && a.DeletadoEm == null)
                .OrderByDescending(a => a.DataInclusao)
                .ToListAsync();
        }

        public async Task<Assinatura?> ObterPorEfiPaySubscriptionIdAsync(string efiPaySubscriptionId)
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .FirstOrDefaultAsync(a => a.EfiPaySubscriptionId == efiPaySubscriptionId
                                       && a.DeletadoEm == null);
        }

        public async Task<bool> ClienteTemAssinaturaAtivaAsync(Guid clienteId)
        {
            return await Context.Assinaturas
                .AnyAsync(a => a.ClienteId == clienteId
                            && a.Ativa
                            && a.DataFim >= DateTime.Now
                            && a.DeletadoEm == null);
        }

        public async Task<List<Assinatura>> ObterAssinaturasProximasDoVencimentoAsync(int dias = 30)
        {
            var dataLimite = DateTime.Now.AddDays(dias);

            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .Where(a => a.Ativa
                         && a.DataFim <= dataLimite
                         && a.DataFim >= DateTime.Now
                         && a.DeletadoEm == null)
                .ToListAsync();
        }

        public async Task<List<Assinatura>> ObterAssinaturasExpiradasAsync()
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .Where(a => a.Ativa
                         && a.DataFim < DateTime.Now
                         && a.DeletadoEm == null)
                .ToListAsync();
        }

        public void Atualizar(Assinatura assinatura)
        {
            Context.Assinaturas.Update(assinatura);
        }

        /// <summary>
        /// Busca assinatura ativa pelo UsuarioId (através do Cliente)
        /// </summary>
        public async Task<Assinatura?> ObterAssinaturaAtivaDoUsuarioAsync(Guid usuarioId)
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .Where(a => a.Cliente != null
                         && a.Cliente.UsuarioID == usuarioId
                         && a.Ativa
                         && a.DataFim >= DateTime.Now
                         && a.DeletadoEm == null)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Busca todas as assinaturas do usuário (via Cliente.UsuarioID ou UsuarioId direto)
        /// </summary>
        public async Task<List<Assinatura>> ObterAssinaturasDoUsuarioAsync(Guid usuarioId)
        {
            return await Context.Assinaturas
                                .Include(a => a.Cliente)
                .Where(a => a.DeletadoEm == null
                         && ((a.Cliente != null && a.Cliente.UsuarioID == usuarioId)
                             || a.UsuarioId == usuarioId))
                .OrderByDescending(a => a.DataInclusao)
                .ToListAsync();
        }

        /// <summary>
        /// Busca assinatura ativa diretamente pelo UsuarioId (assinatura vinculada ao usuario, não cliente)
        /// </summary>
        public async Task<Assinatura?> ObterAssinaturaAtivaPorUsuarioAsync(Guid usuarioId)
        {
            return await Context.Assinaturas
                                .Include(a => a.Usuario)
                .Where(a => a.UsuarioId == usuarioId
                         && a.Ativa
                         && a.DataFim >= DateTime.Now
                         && a.DeletadoEm == null)
                .OrderByDescending(a => a.DataFim)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Busca assinatura ativa unificada - primeiro tenta por UsuarioId direto, depois por Cliente
        /// </summary>
        public async Task<Assinatura?> ObterAssinaturaAtivaUnificadaAsync(Guid usuarioId)
        {
            // Primeiro tenta buscar assinatura vinculada diretamente ao usuario
            var assinaturaUsuario = await ObterAssinaturaAtivaPorUsuarioAsync(usuarioId);
            if (assinaturaUsuario != null)
            {
                return assinaturaUsuario;
            }

            // Fallback: busca assinatura via Cliente
            return await ObterAssinaturaAtivaDoUsuarioAsync(usuarioId);
        }
    }
}
