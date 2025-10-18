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
            return await Context.MensagensAgendadas
                .Where(m => m.Status == StatusMensagem.Pendente && m.DataHoraEnvio <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<MensagemAgendada>> ObterMensagensPorUsuarioAsync(Guid usuarioId)
        {
            return await Context.MensagensAgendadas
                .Where(m => m.UsuarioId == usuarioId)
                .ToListAsync();

        }

        public async Task<List<MensagemAgendada>> ObterMensagensComFiltrosAsync(MensagemAgendadaQueryDTO query)
        {
            var mensagensQuery = Context.MensagensAgendadas.AsQueryable();

            if (query.UsuarioId.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.UsuarioId == query.UsuarioId.Value);
            }

            if (query.Status.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.Status == query.Status.Value);
            }

            if (query.DataInicio.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio >= query.DataInicio.Value);
            }

            if (query.DataFim.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio <= query.DataFim.Value);
            }

            // Paginação
            var skip = ((query.Page ?? 1) - 1) * query.PageSize;
            mensagensQuery = mensagensQuery.Skip(skip).Take(query.PageSize);

            return await mensagensQuery.ToListAsync();


        }

        public async Task<int> ContarMensagensComFiltrosAsync(MensagemAgendadaQueryDTO query)
        {
            var mensagensQuery = Context.MensagensAgendadas.AsQueryable();

            if (query.UsuarioId.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.UsuarioId == query.UsuarioId.Value);
            }

            if (query.Status.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.Status == query.Status.Value);
            }

            if (query.DataInicio.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio >= query.DataInicio.Value);
            }

            if (query.DataFim.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio <= query.DataFim.Value);
            }

            return await mensagensQuery.CountAsync();
        }

        public async Task<List<MensagemAgendada>> ObterTodasMensagensAsync()
        {
            return await Context.MensagensAgendadas
                .Where(m => true)
                .ToListAsync();
        }
        public async Task<List<MensagemAgendada>> ObterTodasMensagensDoUsuarioAsync(Guid usuarioId)
        {
            return await Context.MensagensAgendadas
                .Where(m => m.UsuarioId == usuarioId)
                .ToListAsync();
        }


        public async Task<MensagemAgendada?> ObterPorIdAsync(Guid id)
        {
            return await Context.MensagensAgendadas.FindAsync(id);

        }

        public async Task<int> ContarMensagensPendentesAsync()
        {
            return await Context.MensagensAgendadas
                .Where(m => m.Status == StatusMensagem.Pendente)
                .CountAsync();

        }

        public async Task<List<MensagemAgendada>> ObterMensagensDeFuncionariosDoAdminAsync(Guid adminId, MensagemAgendadaQueryDTO query)
        {
            var mensagensQuery = Context.MensagensAgendadas
                .Where(m => m.Funcionario != null && m.Funcionario.adminId == adminId);

            if (query.FuncionarioId.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.FuncionarioId == query.FuncionarioId.Value);
            }

            if (query.Status.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.Status == query.Status.Value);
            }

            if (query.DataInicio.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio >= query.DataInicio.Value);
            }

            if (query.DataFim.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio <= query.DataFim.Value);
            }

            // Paginação
            var skip = ((query.Page ?? 1) - 1) * query.PageSize;
            mensagensQuery = mensagensQuery.Skip(skip).Take(query.PageSize);

            return await mensagensQuery.ToListAsync();
        }

        public async Task<int> ContarMensagensDeFuncionariosDoAdminAsync(Guid adminId, MensagemAgendadaQueryDTO query)
        {
            var mensagensQuery = Context.MensagensAgendadas
                .Where(m => m.Funcionario != null && m.Funcionario.adminId == adminId);

            if (query.FuncionarioId.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.FuncionarioId == query.FuncionarioId.Value);
            }

            if (query.Status.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.Status == query.Status.Value);
            }

            if (query.DataInicio.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio >= query.DataInicio.Value);
            }

            if (query.DataFim.HasValue)
            {
                mensagensQuery = mensagensQuery.Where(m => m.DataHoraEnvio <= query.DataFim.Value);
            }

            return await mensagensQuery.CountAsync();
        }
    }
}
