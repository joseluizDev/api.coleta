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
    }
}
