using api.cliente.Interfaces;
using api.coleta.Data;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;

namespace api.coleta.Services
{
    public class MensagemAgendadaService : ServiceBase
    {
        private readonly MensagemAgendadaRepository _repository;
        private readonly IOneSignalService _oneSignalService;
        private readonly INotificador _notificador;

        public MensagemAgendadaService(
            IUnitOfWork unitOfWork,
            MensagemAgendadaRepository repository,
            IOneSignalService oneSignalService,
            INotificador notificador) : base(unitOfWork)
        {
            _repository = repository;
            _oneSignalService = oneSignalService;
            _notificador = notificador;
        }

        public async Task<MensagemAgendadaResponseDTO> CriarMensagemAgendadaAsync(MensagemAgendadaRequestDTO dto)
        {
            var mensagemAgendada = new MensagemAgendada(dto);

            _repository.Adicionar(mensagemAgendada);

            var unitOfWorkImplements = UnitOfWork as UnitOfWorkImplements;
            await unitOfWorkImplements!.CommitAsync();

            return ConverterParaResponseDTO(mensagemAgendada);
        }

        public async Task<MensagemAgendadaResponseDTO?> ObterPorIdAsync(Guid id)
        {
            var mensagem = await _repository.ObterPorIdAsync(id);
            return mensagem != null ? ConverterParaResponseDTO(mensagem) : null;
        }

        public async Task<List<MensagemAgendadaResponseDTO>> ObterTodasMensagensAsync()
        {
            var mensagens = await _repository.ObterTodasMensagensAsync();
            return mensagens.Select(ConverterParaResponseDTO).ToList();
        }

        public async Task<List<MensagemAgendadaResponseDTO>> ObterMensagensPorUsuarioAsync(Guid usuarioId)
        {
            var mensagens = await _repository.ObterMensagensPorUsuarioAsync(usuarioId);
            return mensagens.Select(ConverterParaResponseDTO).ToList();
        }

        public async Task<(List<MensagemAgendadaResponseDTO> mensagens, int total)> ObterMensagensComFiltrosAsync(MensagemAgendadaQueryDTO query)
        {
            var mensagens = await _repository.ObterMensagensComFiltrosAsync(query);
            var total = await _repository.ContarMensagensComFiltrosAsync(query);

            return (mensagens.Select(ConverterParaResponseDTO).ToList(), total);
        }

        public async Task<bool> CancelarMensagemAsync(Guid id)
        {
            var mensagem = await _repository.ObterPorIdAsync(id);
            if (mensagem == null)
            {
                _notificador.Notificar(new Notificacao("Mensagem não encontrada"));
                return false;
            }

            if (mensagem.Status != StatusMensagem.Pendente)
            {
                _notificador.Notificar(new Notificacao("Somente mensagens pendentes podem ser canceladas"));
                return false;
            }

            mensagem.Status = StatusMensagem.Cancelada;
            _repository.Atualizar(mensagem);

            var unitOfWorkImplements = UnitOfWork as UnitOfWorkImplements;
            await unitOfWorkImplements!.CommitAsync();

            return true;
        }

        public async Task<bool> AtualizarMensagemAsync(Guid id, MensagemAgendadaRequestDTO dto)
        {
            var mensagemAgendada = await _repository.ObterPorIdAsync(id);
            if (mensagemAgendada == null)
            {
                _notificador.Notificar(new Notificacao("Mensagem não encontrada"));
                return false;
            }

            if (mensagemAgendada.Status != StatusMensagem.Pendente)
            {
                _notificador.Notificar(new Notificacao("Somente mensagens pendentes podem ser editadas"));
                return false;
            }

            mensagemAgendada.Atualizar(dto);
            _repository.Atualizar(mensagemAgendada);

            var unitOfWorkImplements = UnitOfWork as UnitOfWorkImplements;
            await unitOfWorkImplements!.CommitAsync();

            return true;
        }

        public async Task ProcessarMensagensPendentesAsync()
        {
            var mensagens = await _repository.ObterMensagensPendentesParaEnvioAsync();

            foreach (var mensagem in mensagens)
            {
                await EnviarMensagemAsync(mensagem);
            }
        }

        private async Task EnviarMensagemAsync(MensagemAgendada mensagem)
        {
            try
            {
                var unitOfWorkImplements = UnitOfWork as UnitOfWorkImplements;

                if (string.IsNullOrEmpty(mensagem.FcmToken))
                {
                    mensagem.AtualizarStatus(StatusMensagem.Falha, null, "Token FCM não fornecido");
                    _repository.Atualizar(mensagem);
                    await unitOfWorkImplements!.CommitAsync();
                    return;
                }

                var sucesso = await _oneSignalService.EnviarNotificacaoAsync(
                    mensagem.FcmToken,
                    mensagem.Titulo,
                    mensagem.Mensagem
                );

                if (sucesso)
                {
                    mensagem.AtualizarStatus(StatusMensagem.Enviada, DateTime.Now);
                }
                else
                {
                    if (mensagem.TentativasEnvio >= 2)
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Falha, null, "Falha após 3 tentativas de envio");
                    }
                    else
                    {
                        mensagem.AtualizarStatus(StatusMensagem.Falha, null, $"Tentativa {mensagem.TentativasEnvio + 1} falhou");
                    }
                }

                _repository.Atualizar(mensagem);
                await unitOfWorkImplements!.CommitAsync();
            }
            catch (Exception ex)
            {
                var unitOfWorkImplements = UnitOfWork as UnitOfWorkImplements;

                mensagem.AtualizarStatus(StatusMensagem.Falha, null, ex.Message);
                _repository.Atualizar(mensagem);
                await unitOfWorkImplements!.CommitAsync();
            }
        }

        public async Task<int> ContarMensagensPendentesAsync()
        {
            return await _repository.ContarMensagensPendentesAsync();
        }

        public async Task<bool> MarcarComoLidaAsync(Guid mensagemId, Guid usuarioId)
        {
            var mensagem = await _repository.ObterPorIdAsync(mensagemId);

            if (mensagem == null)
            {
                _notificador.Notificar(new Notificacao("Mensagem não encontrada"));
                return false;
            }

            // Verifica se a mensagem é destinada ao usuário
            if (mensagem.UsuarioId != usuarioId)
            {
                _notificador.Notificar(new Notificacao("Você não tem permissão para marcar esta mensagem como lida"));
                return false;
            }

            // Só pode marcar como lida se estiver no status Enviada
            if (mensagem.Status != StatusMensagem.Enviada)
            {
                _notificador.Notificar(new Notificacao("Apenas mensagens enviadas podem ser marcadas como lidas"));
                return false;
            }

            mensagem.Status = StatusMensagem.Lida;
            _repository.Atualizar(mensagem);

            var unitOfWorkImplements = UnitOfWork as UnitOfWorkImplements;
            await unitOfWorkImplements!.CommitAsync();

            return true;
        }

        public async Task<List<MensagemAgendadaResponseDTO>> ObterMensagensDoUsuarioAsync(Guid usuarioId, bool apenasNaoLidas = false)
        {
            var query = new MensagemAgendadaQueryDTO
            {
                UsuarioId = usuarioId
            };

            var mensagens = await _repository.ObterMensagensComFiltrosAsync(query);

            if (apenasNaoLidas)
            {
                mensagens = mensagens.Where(m => m.Status != StatusMensagem.Lida).ToList();
            }

            return mensagens.Select(ConverterParaResponseDTO).ToList();
        }

        public async Task<object> ObterEstatisticasAsync(Guid funcionarioId)
        {
            var query = new MensagemAgendadaQueryDTO
            {
                FuncionarioId = funcionarioId
            };

            var todas = await _repository.ObterMensagensComFiltrosAsync(query);

            return new
            {
                total = todas.Count,
                totalPendentes = todas.Count(m => m.Status == StatusMensagem.Pendente),
                totalEnviadas = todas.Count(m => m.Status == StatusMensagem.Enviada),
                totalFalhas = todas.Count(m => m.Status == StatusMensagem.Falha),
                totalCanceladas = todas.Count(m => m.Status == StatusMensagem.Cancelada)
            };
        }

        private MensagemAgendadaResponseDTO ConverterParaResponseDTO(MensagemAgendada mensagem)
        {
            return new MensagemAgendadaResponseDTO
            {
                Id = mensagem.Id,
                Titulo = mensagem.Titulo,
                Mensagem = mensagem.Mensagem,
                DataHoraEnvio = mensagem.DataHoraEnvio,
                DataHoraEnviada = mensagem.DataHoraEnviada,
                Status = mensagem.Status,
                FcmToken = mensagem.FcmToken,
                UsuarioId = mensagem.UsuarioId,
                FuncionarioId = mensagem.FuncionarioId,
                MensagemErro = mensagem.MensagemErro,
                TentativasEnvio = mensagem.TentativasEnvio
            };
        }
    }
}
