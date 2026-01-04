using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Assinatura : Entity
    {
        // Cliente (opcional - para assinaturas empresariais)
        [ForeignKey("Cliente")]
        public Guid? ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        // Usuario (opcional - para assinaturas diretas de usuário)
        [ForeignKey("Usuario")]
        public Guid? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

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

        // Valida que a assinatura tem exatamente um vínculo (Usuario OU Cliente)
        public bool ValidarVinculo()
        {
            bool temCliente = ClienteId.HasValue && ClienteId.Value != Guid.Empty;
            bool temUsuario = UsuarioId.HasValue && UsuarioId.Value != Guid.Empty;

            // Deve ter exatamente um dos dois (XOR)
            return temCliente ^ temUsuario;
        }

        // Retorna a identificação do assinante (nome)
        public string ObterIdentificacao()
        {
            if (Usuario != null)
                return Usuario.NomeCompleto ?? "Usuário";
            if (Cliente != null)
                return Cliente.Nome ?? "Cliente";
            return "Não identificado";
        }

        // Retorna o documento do assinante (CPF/CNPJ)
        public string? ObterDocumento()
        {
            if (Usuario != null)
                return Usuario.CPF;
            if (Cliente != null)
                return Cliente.Documento;
            return null;
        }

        // Retorna o email do assinante
        public string? ObterEmail()
        {
            if (Usuario != null)
                return Usuario.Email;
            if (Cliente != null)
                return Cliente.Email;
            return null;
        }
    }
}
