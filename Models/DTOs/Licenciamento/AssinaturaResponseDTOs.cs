namespace api.coleta.Models.DTOs.Licenciamento
{
    /// <summary>
    /// Resposta de status da licenca para o frontend
    /// </summary>
    public class StatusLicencaResponseDTO
    {
        public bool TemLicenca { get; set; }
        public bool LicencaAtiva { get; set; }
        public string StatusMensagem { get; set; } = string.Empty;
        public int DiasRestantes { get; set; }
        public object? Plano { get; set; }
        public string? Fonte { get; set; }
        public List<string>? Alertas { get; set; }
    }

    /// <summary>
    /// Resposta de criacao de assinatura com PIX
    /// </summary>
    public class AssinaturaPixResponseDTO
    {
        public Guid? AssinaturaId { get; set; }
        public string? QrCode { get; set; }
        public string? QrCodeImagem { get; set; }
        public string? TxId { get; set; }
        public DateTime DataExpiracao { get; set; }
    }

    /// <summary>
    /// Resposta de criacao de assinatura com Boleto
    /// </summary>
    public class AssinaturaBoletoResponseDTO
    {
        public Guid? AssinaturaId { get; set; }
        public string? CodigoBarras { get; set; }
        public string? Url { get; set; }
        public string? TxId { get; set; }
        public DateTime DataVencimento { get; set; }
    }

    /// <summary>
    /// Resposta de criacao de assinatura com Pix Automatico
    /// </summary>
    public class AssinaturaPixAutomaticoResponseDTO
    {
        public Guid? AssinaturaId { get; set; }
        public string? IdRec { get; set; }
        public string? Contrato { get; set; }
        public string? StatusRecorrencia { get; set; }
        public string? QrCode { get; set; }
        public string? QrCodeImagem { get; set; }
        public string? LinkAutorizacao { get; set; }
        public string? TxId { get; set; }
    }

    /// <summary>
    /// Resposta de criacao de assinatura com Cartao
    /// </summary>
    public class AssinaturaCartaoResponseDTO
    {
        public Guid? AssinaturaId { get; set; }
        public bool Ativa { get; set; }
        public string? ChargeId { get; set; }
        public string? Status { get; set; }
        public string? Autorizacao { get; set; }
        public int Parcelas { get; set; }
        public decimal? ValorTotal { get; set; }
        public string? TxId { get; set; }
    }

    /// <summary>
    /// Opcao de parcelamento para cartao de credito
    /// </summary>
    public class ParcelaDTO
    {
        public int Parcela { get; set; }
        public int Valor { get; set; } // Centavos
        public decimal ValorReais { get; set; }
        public string ValorFormatado { get; set; } = string.Empty;
        public int ValorTotal { get; set; } // Centavos
        public decimal ValorTotalReais { get; set; }
        public string ValorTotalFormatado { get; set; } = string.Empty;
        public bool TemJuros { get; set; }
        public decimal PercentualJuros { get; set; }
    }

    /// <summary>
    /// Resposta com opcoes de parcelamento
    /// </summary>
    public class ParcelamentoResponseDTO
    {
        public List<ParcelaDTO> Parcelas { get; set; } = new();
        public string Bandeira { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado generico do service (sucesso ou erro)
    /// </summary>
    public class AssinaturaResultDTO<T>
    {
        public bool Sucesso { get; set; }
        public T? Dados { get; set; }
        public string? Erro { get; set; }

        public static AssinaturaResultDTO<T> Ok(T dados) => new() { Sucesso = true, Dados = dados };
        public static AssinaturaResultDTO<T> Falha(string erro) => new() { Sucesso = false, Erro = erro };
    }
}
