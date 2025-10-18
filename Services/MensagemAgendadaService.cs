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
                Mensagem = request.Mensagem,
                Status = StatusMensagem.Pendente,
                TentativasEnvio = 0
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
            var result = _repository.ObterMensagensPorUsuario(usuarioId).ToList();

            return new MensagemAgendadaEstatisticasDTO
            {
                Total = result.Count,
                TotalPendentes = result.Count(r => r.Status == StatusMensagem.Pendente),
                TotalEnviadas = result.Count(r => r.Status == StatusMensagem.Enviada),
                TotalFalhas = result.Count(r => r.Status == StatusMensagem.Falha),
                TotalCanceladas = result.Count(r => r.Status == StatusMensagem.Cancelada)
            };
        }

        public MensagemAgendada? ObterMensagemPorId(Guid id, Guid usuarioId)
        {
            var mensagem = _repository.ObterPorId(id);

            // Verifica se a mensagem existe e pertence ao usuário
            if (mensagem == null || mensagem.UsuarioId != usuarioId)
            {
                return null;
            }

            return mensagem;
        }

        public bool AtualizarMensagem(Guid id, MensagemAgendadaRequestDTO request, Guid usuarioId)
        {
            var mensagem = _repository.ObterPorId(id);

            // Verifica se a mensagem existe e pertence ao usuário
            if (mensagem == null || mensagem.UsuarioId != usuarioId)
            {
                return false;
            }

            // Não permite atualizar mensagens já enviadas
            if (mensagem.Status == StatusMensagem.Enviada)
            {
                _notificador.Notificar(new Notificacao("Não é possível atualizar uma mensagem já enviada."));
                return false;
            }

            // Atualiza apenas os campos permitidos
            mensagem.Titulo = request.Titulo;
            mensagem.Mensagem = request.Mensagem;
            mensagem.DataHoraEnvio = request.DataHoraEnvio;
            mensagem.FuncionarioId = request.FuncionarioId;

            _repository.Atualizar(mensagem);
            UnitOfWork.Commit();

            return true;
        }

        public bool CancelarMensagem(Guid id, Guid usuarioId)
        {
            var mensagem = _repository.ObterPorId(id);

            // Verifica se a mensagem existe e pertence ao usuário
            if (mensagem == null || mensagem.UsuarioId != usuarioId)
            {
                return false;
            }

            // Não permite cancelar mensagens já enviadas ou lidas
            if (mensagem.Status == StatusMensagem.Enviada || mensagem.Status == StatusMensagem.Lida)
            {
                _notificador.Notificar(new Notificacao("Não é possível cancelar uma mensagem já enviada ou lida."));
                return false;
            }

            // Marca como cancelada ao invés de deletar
            mensagem.Status = StatusMensagem.Cancelada;
            _repository.Atualizar(mensagem);
            UnitOfWork.Commit();

            return true;
        }

        public async Task ProcessarMensagensPendentesAsync()
        {
            var mensagensPendentes = await _repository.ObterMensagensPendentesAsync();

            foreach (var mensagem in mensagensPendentes)
            {
                try
                {
                    // Validar se tem FuncionarioId
                    if (!mensagem.FuncionarioId.HasValue)
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Falha, null, "FuncionarioId não informado");
                        _repository.Atualizar(mensagem);
                        continue;
                    }

                    // Buscar o usuário (funcionário) relacionado para obter o FCM token
                    var funcionario = mensagem.Funcionario;

                    if (funcionario == null && mensagem.FuncionarioId.HasValue)
                    {
                        if (mensagem.UsuarioId.HasValue)
                        {
                            funcionario = _usuarioRepository.ObterFuncionario(mensagem.FuncionarioId.Value, mensagem.UsuarioId.Value);
                        }

                        funcionario ??= await _usuarioRepository.ObterPorIdAsync(mensagem.FuncionarioId.Value);
                    }

                    // Validar se o funcionário existe e tem FcmToken
                    if (funcionario == null)
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Falha, null, "Funcionário não encontrado");
                        _repository.Atualizar(mensagem);
                        continue;
                    }

                    mensagem.Funcionario = funcionario;

                    var fcmToken = funcionario.FcmToken;

                    if (string.IsNullOrWhiteSpace(fcmToken))
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Falha, null, "FcmToken do funcionário não encontrado");
                        _repository.Atualizar(mensagem);
                        continue;
                    }

                    // Enviar notificação
                    var enviada = await _oneSignalService.EnviarNotificacaoAsync(fcmToken, mensagem.Titulo, mensagem.Mensagem);

                    if (enviada)
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Enviada, DateTime.Now);
                    }
                    else
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Falha, null, "Falha ao enviar notificação via OneSignal");
                    }

                    _repository.Atualizar(mensagem);
                }
                catch (Exception ex)
                {
                    mensagem.AtualizarStatus(StatusMensagem.Falha, null, $"Erro ao processar mensagem: {ex.Message}");
                    _repository.Atualizar(mensagem);
                }
            }

            if (mensagensPendentes.Any())
            {
                UnitOfWork.Commit();
            }
        }
    }
}
