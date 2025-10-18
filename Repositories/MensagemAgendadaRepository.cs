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
    }
}
