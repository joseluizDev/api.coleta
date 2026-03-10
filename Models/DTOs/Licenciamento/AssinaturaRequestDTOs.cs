namespace api.coleta.Models.DTOs.Licenciamento
{
    public class AtivarAssinaturaDTO
    {
        public string? Observacao { get; set; }
    }

    public class CriarAssinaturaPixDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }
    }

    public class CriarAssinaturaUsuarioPixDTO
    {
        public Guid PlanoId { get; set; }
    }

    public class CriarAssinaturaBoletoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }
        public string NomePagador { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
    }

    public class CriarAssinaturaUsuarioBoletoDTO
    {
        public Guid PlanoId { get; set; }
        public string NomePagador { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
    }

    public class CriarAssinaturaPixAutomaticoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }
        public string NomeDevedor { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Periodicidade { get; set; } = "ANUAL";
    }

    public class CriarAssinaturaUsuarioPixAutomaticoDTO
    {
        public Guid PlanoId { get; set; }
        public string NomeDevedor { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Periodicidade { get; set; } = "ANUAL";
    }

    public class CriarAssinaturaCartaoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }

        // Modo recomendado: payment_token do SDK EfiPay
        public string? PaymentToken { get; set; }
        public string? NomePagador { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }

        // Modo legado: dados do cartao diretamente
        public string? NumeroCartao { get; set; }
        public string? Cvv { get; set; }
        public string? MesValidade { get; set; }
        public string? AnoValidade { get; set; }
        public string? NomeCartao { get; set; }

        // Comum a ambos os modos
        public int Parcelas { get; set; } = 1;
        public string? Bandeira { get; set; }
    }

    public class CriarAssinaturaUsuarioCartaoDTO
    {
        public Guid PlanoId { get; set; }

        // Modo recomendado: payment_token do SDK EfiPay
        public string? PaymentToken { get; set; }
        public string? NomePagador { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }

        // Modo legado: dados do cartao diretamente
        public string? NumeroCartao { get; set; }
        public string? Cvv { get; set; }
        public string? MesValidade { get; set; }
        public string? AnoValidade { get; set; }
        public string? NomeCartao { get; set; }

        // Comum a ambos os modos
        public int Parcelas { get; set; } = 1;
        public string? Bandeira { get; set; }
    }
}
