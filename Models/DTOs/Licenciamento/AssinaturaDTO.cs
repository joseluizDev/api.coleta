using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.DTOs.Licenciamento
{
    public class AssinaturaDTO
    {
        public Guid Id { get; set; }
        public Guid? ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public Guid? UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public Guid PlanoId { get; set; }
        public PlanoDTO? Plano { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativa { get; set; }
        public bool AutoRenovar { get; set; }
        public string? Observacao { get; set; }
        public bool EstaVigente { get; set; }
        public int DiasRestantes { get; set; }
        public string? StatusPagamento { get; set; }
    }

    public class AssinaturaCreateDTO
    {
        // ClienteId é opcional - se não fornecido, usa UsuarioId do token
        public Guid? ClienteId { get; set; }

        [Required(ErrorMessage = "Plano é obrigatório")]
        public Guid PlanoId { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Data de fim é obrigatória")]
        public DateTime DataFim { get; set; }

        public bool AutoRenovar { get; set; } = false;

        public string? Observacao { get; set; }
    }

    // DTO específico para criação de assinatura vinculada a usuário
    public class AssinaturaCreateUsuarioDTO
    {
        [Required(ErrorMessage = "Plano é obrigatório")]
        public Guid PlanoId { get; set; }

        public string? CpfCnpj { get; set; }
    }

    public class AssinaturaCreateComPixDTO : AssinaturaCreateDTO
    {
        // CPF/CNPJ agora é opcional no request - será preenchido automaticamente do usuário logado
        public string? CpfCnpj { get; set; }
    }

    public class AssinaturaComPagamentoDTO
    {
        public AssinaturaDTO Assinatura { get; set; } = null!;
        public PagamentoPixDTO? Pagamento { get; set; }
    }

    public class PagamentoPixDTO
    {
        public string TxId { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public string QrCodeImagem { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataExpiracao { get; set; }
    }

    public class VerificacaoPagamentoDTO
    {
        public Guid AssinaturaId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool Pago { get; set; }
        public bool AssinaturaAtiva { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVerificacao { get; set; }
    }

    public class HistoricoPagamentoDTO
    {
        public Guid Id { get; set; }
        public Guid AssinaturaId { get; set; }
        /// <summary>
        /// ID do plano no Gateway PostgreSQL (detalhes devem ser consultados via gateway)
        /// </summary>
        public Guid PlanoId { get; set; }
        public string? PlanoNome { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Ativa { get; set; }
        public string StatusPagamento { get; set; } = string.Empty;
        public int DiasRestantes { get; set; }
        public string MetodoPagamento { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? EfiPayStatus { get; set; }
        public string? PixTxId { get; set; }
        public int? CartaoParcelas { get; set; }
        public string? CartaoBandeira { get; set; }
    }
}
