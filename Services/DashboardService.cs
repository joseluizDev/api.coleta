using api.coleta.Data;
using api.coleta.Services;
using api.dashboard.DTOs;

namespace api.dashboard.Services
{
    public class DashboardService : ServiceBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _context = context;
        }

        public DashboardResumoDTO ObterResumo(Guid userId)
        {
            var resumo = new DashboardResumoDTO
            {
                Clientes = _context.Clientes.Count(c => c.UsuarioID == userId),
                Fazendas = _context.Fazendas.Count(f => f.UsuarioID == userId),
                SafrasAtivas = _context.Safras.Count(s => s.UsuarioID == userId && s.DataFim == null),
                Talhoes = _context.Talhoes.Count(t => t.UsuarioID == userId),
                Funcionarios = _context.Usuarios.Count(u => u.adminId == userId)
            };

            return resumo;
        }
    }
}
