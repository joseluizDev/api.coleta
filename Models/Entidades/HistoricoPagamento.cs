using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public enum StatusPagamento
    {
        Pendente = 0,
        Aprovado = 1,
        Recusado = 2,
        Cancelado = 3,
        Estornado = 4
    }

    public class HistoricoPagamento : Entity
    {
        [Required]
        [ForeignKey("Assinatura")]
        public Guid AssinaturaId { get; set; }
        public virtual Assinatura Assinatura { get; set; } = null!;

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public DateTime DataPagamento { get; set; }

        [Required]
        [MaxLength(50)]
        public string MetodoPagamento { get; set; } = string.Empty;

        [Required]
        public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

        [MaxLength(255)]
        public string? TransacaoId { get; set; }

        public string? Observacao { get; set; }

        public DateTime? DeletadoEm { get; set; }

        // EfiPay PIX Fields
        [MaxLength(100)]
        public string? EfiPayChargeId { get; set; }

        [MaxLength(50)]
        public string? EfiPayStatus { get; set; }

        public string? PixQrCode { get; set; }

        public string? PixQrCodeBase64 { get; set; }

        [MaxLength(100)]
        public string? PixTxId { get; set; }

        public DateTime? DataExpiracao { get; set; }

        // EfiPay Boleto Fields
        [MaxLength(100)]
        public string? BoletoCodigoBarras { get; set; }

        [MaxLength(100)]
        public string? BoletoLinhaDigitavel { get; set; }

        public string? BoletoPdfUrl { get; set; }

        public string? BoletoLink { get; set; }

        public DateTime? BoletoVencimento { get; set; }

        // EfiPay Cartão Fields
        [MaxLength(4)]
        public string? CartaoUltimos4Digitos { get; set; }

        [MaxLength(20)]
        public string? CartaoBandeira { get; set; }

        public int? CartaoParcelas { get; set; }

        public decimal? CartaoValorParcela { get; set; }

        // EfiPay Assinatura Recorrente Fields
        [MaxLength(100)]
        public string? EfiPaySubscriptionId { get; set; }

        public int? RecorrenciaParcela { get; set; }  // 1 de 12, 2 de 12, etc.

        public int? RecorrenciaTotalParcelas { get; set; }

        public HistoricoPagamento() { }

        public bool PixExpirado()
        {
            return DataExpiracao.HasValue && DateTime.Now > DataExpiracao.Value;
        }
    }
}
