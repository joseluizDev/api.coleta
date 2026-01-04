namespace api.coleta.Models.DTOs.Licenciamento
{
    public class LicenseStatusDTO
    {
        public bool TemLicenca { get; set; }
        public bool LicencaAtiva { get; set; }
        public string StatusMensagem { get; set; } = string.Empty;
        public PlanoDTO? PlanoAtual { get; set; }
        public AssinaturaDTO? AssinaturaAtual { get; set; }

        // Hectares usage
        public decimal HectaresUtilizados { get; set; }
        public decimal HectaresDisponiveis { get; set; }
        public decimal PercentualUtilizado { get; set; }

        // Expiration info
        public int DiasRestantes { get; set; }
        public bool ProximoDoVencimento { get; set; }
        public DateTime? DataVencimento { get; set; }

        // Alerts
        public List<string> Alertas { get; set; } = new();
    }

    public class ValidacaoLicencaResult
    {
        public bool Valida { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public int? DiasRestantes { get; set; }
        public Guid? ClienteId { get; set; }
        public Guid? AssinaturaId { get; set; }
    }
}
