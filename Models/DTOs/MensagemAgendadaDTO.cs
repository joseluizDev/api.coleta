using api.coleta.Models.Entidades;

namespace api.coleta.Models.DTOs
{
    public class MensagemAgendadaRequestDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataHoraEnvio { get; set; }
        public string? FcmToken { get; set; }
        
        public Guid? UsuarioId { get; set; }
        public Guid? FuncionarioId { get; set; }
    }

    public class MensagemAgendadaResponseDTO
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataHoraEnvio { get; set; }
        public DateTime? DataHoraEnviada { get; set; }
        public StatusMensagem Status { get; set; }
        public string? FcmToken { get; set; }
        public Guid? UsuarioId { get; set; }
        public Guid? FuncionarioId { get; set; }
        public string? MensagemErro { get; set; }
        public int TentativasEnvio { get; set; }
    }

    public class MensagemAgendadaUpdateStatusDTO
    {
        public Guid Id { get; set; }
        public StatusMensagem Status { get; set; }
        public DateTime? DataHoraEnviada { get; set; }
        public string? MensagemErro { get; set; }
        public int TentativasEnvio { get; set; }
    }

    public class MensagemAgendadaQueryDTO
    {
        public Guid? FuncionarioId { get; set; }
        public Guid? UsuarioId { get; set; }
        public StatusMensagem? Status { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? Page { get; set; }
        public int PageSize { get; set; } = 50;
    }

    public class MensagemAgendadaEstatisticasDTO
    {
        public int Total { get; set; }
        public int TotalPendentes { get; set; }
        public int TotalEnviadas { get; set; }
        public int TotalFalhas { get; set; }
        public int TotalCanceladas { get; set; }
    }
}
