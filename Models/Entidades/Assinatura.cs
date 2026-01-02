using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Assinatura : Entity
    {
        [Required]
        [ForeignKey("Cliente")]
        public Guid ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; } = null!;

        [Required]
        [ForeignKey("Plano")]
        public Guid PlanoId { get; set; }
        public virtual Plano Plano { get; set; } = null!;

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        public bool Ativa { get; set; } = false;

        public bool AutoRenovar { get; set; } = false;

        public string? Observacao { get; set; }

        public DateTime? DeletadoEm { get; set; }

        // EfiPay Integration Fields
        [MaxLength(100)]
        public string? EfiPaySubscriptionId { get; set; }

        [MaxLength(50)]
        public string? EfiPayPlanId { get; set; }

        [MaxLength(50)]
        public string? StatusPagamento { get; set; }

        public DateTime? DataUltimoPagamento { get; set; }

        // Navigation
        public virtual ICollection<HistoricoPagamento> Pagamentos { get; set; } = new List<HistoricoPagamento>();

        public Assinatura() { }

        // Business Logic Methods
        public bool EstaVigente()
        {
            return Ativa && DataFim >= DateTime.Now && DeletadoEm == null;
        }

        public bool EstaDentroDoLimiteHectares(decimal totalHectares)
        {
            return Plano != null && totalHectares <= Plano.LimiteHectares;
        }

        public int DiasRestantes()
        {
            if (!EstaVigente()) return 0;
            return (DataFim - DateTime.Now).Days;
        }

        public bool ProximoDoVencimento(int diasAlerta = 30)
        {
            return EstaVigente() && DiasRestantes() <= diasAlerta;
        }
    }
}
