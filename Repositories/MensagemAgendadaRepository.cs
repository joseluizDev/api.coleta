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

        public List<MensagemAgendada> ObterMensagensPorUsuario(Guid usuarioId)
        {
            return Context.MensagensAgendadas
                .Where(m => m.UsuarioId == usuarioId)
                 .OrderByDescending(m => m.DataHoraEnvio)
                 .ToList();
        }

        public async Task<List<MensagemAgendada>> ObterTodasAsync()
        {
            return await Context.MensagensAgendadas.ToListAsync();
        }

        public async Task<MensagemAgendada?> ObterPorIdAsync(Guid id)
        {
            return await Context.MensagensAgendadas
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MensagemAgendada>> ObterMensagensPendentesAsync()
        {
            var agora = DateTime.Now;
            return await Context.MensagensAgendadas
                .Include(m => m.Funcionario)
                .Where(m => m.Status == StatusMensagem.Pendente && m.DataHoraEnvio <= agora)
                .ToListAsync();
        }
    }
}
