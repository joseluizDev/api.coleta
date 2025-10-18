using api.coleta.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class MensagemAgendada : Entity
    {
        public MensagemAgendada()
        {
            Status = StatusMensagem.Pendente;
            TentativasEnvio = 0;
        }

        public MensagemAgendada(MensagemAgendadaRequestDTO dto)
        {
            Titulo = dto.Titulo;
            Mensagem = dto.Mensagem;
            DataHoraEnvio = dto.DataHoraEnvio;
            FcmToken = dto.FcmToken;
            UsuarioId = dto.UsuarioId;
            FuncionarioId = dto.FuncionarioId;
            Status = StatusMensagem.Pendente;
            TentativasEnvio = 0;
        }

        public MensagemAgendada Atualizar(MensagemAgendadaRequestDTO dto)
        {
            Titulo = dto.Titulo;
            Mensagem = dto.Mensagem;
            DataHoraEnvio = dto.DataHoraEnvio;
            FcmToken = dto.FcmToken;
            UsuarioId = dto.UsuarioId;
            FuncionarioId = dto.FuncionarioId;
            return this;
        }

        public MensagemAgendada AtualizarStatus(StatusMensagem novoStatus, DateTime? dataHoraEnviada = null, string? mensagemErro = null)
        {
            Status = novoStatus;

            if (novoStatus == StatusMensagem.Enviada)
            {
                DataHoraEnviada = dataHoraEnviada ?? DateTime.Now;
                MensagemErro = null;
            }
            else if (novoStatus == StatusMensagem.Falha)
            {
                TentativasEnvio++;
                MensagemErro = mensagemErro;
            }

            return this;
        }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Mensagem { get; set; } = string.Empty;

        [Required]
        public DateTime DataHoraEnvio { get; set; }

        public DateTime? DataHoraEnviada { get; set; }

        [Required]
        public StatusMensagem Status { get; set; }

        [MaxLength(500)]
        public string? FcmToken { get; set; }

        [ForeignKey("Usuario")]
        public Guid? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("Funcionario")]
        public Guid? FuncionarioId { get; set; }
        public virtual Usuario? Funcionario { get; set; }

        [MaxLength(1000)]
        public string? MensagemErro { get; set; }

        public int TentativasEnvio { get; set; } = 0;
    }
}
