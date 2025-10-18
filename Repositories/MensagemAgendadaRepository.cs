using api.coleta.Data;
using api.coleta.Data.Repositories;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class MensagemAgendadaRepository : GenericRepository<MensagemAgendada>
    {
        public MensagemAgendadaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MensagemAgendada>> ObterMensagensPendentesParaEnvioAsync()
        {
            var agora = DateTime.Now;

            return await DbSet
                .Where(m => m.Status == StatusMensagem.Pendente
                         && m.DataHoraEnvio <= agora)
                .OrderBy(m => m.DataHoraEnvio)
                .ToListAsync();
        }

        public async Task<List<MensagemAgendada>> ObterMensagensPorUsuarioAsync(Guid usuarioId)
        {
            return await DbSet
                .Where(m => m.UsuarioId == usuarioId)
                .OrderByDescending(m => m.DataHoraEnvio)
                .ToListAsync();
        }

        public async Task<List<MensagemAgendada>> ObterMensagensComFiltrosAsync(MensagemAgendadaQueryDTO query)
        {
            var queryable = DbSet.AsQueryable();

            // Filtro por funcionário que criou/enviou
            if (query.FuncionarioId.HasValue)
            {
                queryable = queryable.Where(m => m.FuncionarioId == query.FuncionarioId.Value);
            }

            // Filtro por usuário destinatário
            if (query.UsuarioId.HasValue)
            {
                queryable = queryable.Where(m => m.UsuarioId == query.UsuarioId.Value);
            }

            // Filtro por status
            if (query.Status.HasValue)
            {
                queryable = queryable.Where(m => m.Status == query.Status.Value);
            }

            // Filtro por data de envio (início)
            if (query.DataInicio.HasValue)
            {
                queryable = queryable.Where(m => m.DataHoraEnvio >= query.DataInicio.Value);
            }

            // Filtro por data de envio (fim)
            if (query.DataFim.HasValue)
            {
                queryable = queryable.Where(m => m.DataHoraEnvio <= query.DataFim.Value);
            }

            // Ordenação
            queryable = queryable.OrderByDescending(m => m.DataHoraEnvio);

            // Paginação
            if (query.Page.HasValue && query.Page.Value > 0)
            {
                queryable = queryable
                    .Skip((query.Page.Value - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await queryable.ToListAsync();
        }

        public async Task<int> ContarMensagensComFiltrosAsync(MensagemAgendadaQueryDTO query)
        {
            var queryable = DbSet.AsQueryable();

            if (query.FuncionarioId.HasValue)
            {
                queryable = queryable.Where(m => m.FuncionarioId == query.FuncionarioId.Value);
            }

            if (query.UsuarioId.HasValue)
            {
                queryable = queryable.Where(m => m.UsuarioId == query.UsuarioId.Value);
            }

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(m => m.Status == query.Status.Value);
            }

            if (query.DataInicio.HasValue)
            {
                queryable = queryable.Where(m => m.DataHoraEnvio >= query.DataInicio.Value);
            }

            if (query.DataFim.HasValue)
            {
                queryable = queryable.Where(m => m.DataHoraEnvio <= query.DataFim.Value);
            }

            return await queryable.CountAsync();
        }

        public async Task<List<MensagemAgendada>> ObterTodasMensagensAsync()
        {
            return await DbSet
                .OrderByDescending(m => m.DataHoraEnvio)
                .ToListAsync();
        }

        public async Task<MensagemAgendada?> ObterPorIdAsync(Guid id)
        {
            return await DbSet.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> ContarMensagensPendentesAsync()
        {
            return await DbSet.CountAsync(m => m.Status == StatusMensagem.Pendente);
        }
    }
}
