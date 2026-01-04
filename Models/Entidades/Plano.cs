using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.Entidades
{
    public class Plano : Entity
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public decimal ValorAnual { get; set; }

        [Required]
        public decimal LimiteHectares { get; set; }

        public bool Ativo { get; set; } = true;

        public bool RequereContato { get; set; } = false;

        [MaxLength(50)]
        public string? EfiPayPlanId { get; set; }

        // ID numérico do plano na EfiPay (para assinaturas recorrentes)
        public int? EfiPayPlanIdInt { get; set; }

        // Valor mensal calculado (ValorAnual / 12)
        public decimal ValorMensal => Math.Round(ValorAnual / 12, 2);

        public virtual ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();

        public Plano() { }

        public Plano(string nome, string descricao, decimal valorAnual, decimal limiteHectares, bool requereContato = false)
        {
            Nome = nome;
            Descricao = descricao;
            ValorAnual = valorAnual;
            LimiteHectares = limiteHectares;
            RequereContato = requereContato;
        }
    }
}
