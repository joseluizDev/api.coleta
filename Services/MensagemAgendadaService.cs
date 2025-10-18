using api.cliente.Interfaces;
using api.coleta.Data;
using api.coleta.Data.Repository;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;

namespace api.coleta.Services
{
    public class MensagemAgendadaService : ServiceBase
    {
        private readonly MensagemAgendadaRepository _repository;
        private readonly UsuarioRepository _usuarioRepository;
        private readonly IOneSignalService _oneSignalService;
        private readonly INotificador _notificador;

        public MensagemAgendadaService(
            IUnitOfWork unitOfWork,
            MensagemAgendadaRepository repository,
            UsuarioRepository usuarioRepository,
            IOneSignalService oneSignalService,
            INotificador notificador) : base(unitOfWork)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _oneSignalService = oneSignalService;
            _notificador = notificador;
        }

        public void CriarMensagemAgendada(MensagemAgendadaRequestDTO request)
        {
            var mensagem = new MensagemAgendada
            {
                UsuarioId = request.UsuarioId,
                Titulo = request.Titulo,
                FuncionarioId = request.FuncionarioId,
                DataHoraEnvio = request.DataHoraEnvio,
                FcmToken = request.FcmToken,
                Mensagem = request.Mensagem,
                Status = StatusMensagem.Pendente,
                TentativasEnvio = 0,

            };
            _repository.Adicionar(mensagem);
            UnitOfWork.Commit();

        }

        public List<MensagemAgendada> ObterMensagensPorUsuario(Guid value)
        {
            return _repository.ObterMensagensPorUsuario(value);
        }

        public async Task<List<MensagemAgendada>> ObterMensagensDoFuncionarioAsync(Guid funcionarioId, bool apenasNaoLidas)
        {
            var mensagens = await _repository.ObterTodasAsync();

            var query = mensagens.Where(m => m.FuncionarioId == funcionarioId);

            if (apenasNaoLidas)
            {
                query = query.Where(m => m.Status == StatusMensagem.Enviada);
            }

            return query.OrderByDescending(m => m.DataHoraEnvio).ToList();
        }

        public async Task<MensagemAgendada?> ObterPorIdAsync(Guid id)
        {
            return await _repository.ObterPorIdAsync(id);
        }

        public async Task<bool> MarcarComoLidaAsync(Guid id, Guid funcionarioId)
        {
            var mensagem = await _repository.ObterPorIdAsync(id);

            if (mensagem == null || mensagem.FuncionarioId != funcionarioId)
            {
                return false;
            }

            mensagem.Status = StatusMensagem.Lida;
            _repository.Atualizar(mensagem);
            UnitOfWork.Commit();

            return true;
        }

        public MensagemAgendadaEstatisticasDTO ObterEstatisticas(Guid usuarioId)
        {
            var result = _repository.ObterMensagensPorUsuario(usuarioId)
                .GroupBy(m => m.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return new MensagemAgendadaEstatisticasDTO
            {
                Total = result.Sum(r => r.Count),
                TotalPendentes = result.FirstOrDefault(r => r.Status == StatusMensagem.Pendente)?.Count ?? 0,
                TotalEnviadas = result.FirstOrDefault(r => r.Status == StatusMensagem.Enviada)?.Count ?? 0,
                TotalFalhas = result.FirstOrDefault(r => r.Status == StatusMensagem.Falha)?.Count ?? 0,
                TotalCanceladas = result.FirstOrDefault(r => r.Status == StatusMensagem.Cancelada)?.Count ?? 0
            };
        }
    }
}
