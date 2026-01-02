using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.DTOs.Licenciamento
{
    public class PlanoDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorAnual { get; set; }
        public decimal LimiteHectares { get; set; }
        public bool Ativo { get; set; }
        public bool RequereContato { get; set; }
        public int? EfiPayPlanIdInt { get; set; }

        public string ValorFormatado => ValorAnual > 0
            ? ValorAnual.ToString("C", new System.Globalization.CultureInfo("pt-BR"))
            : "Sob consulta";

        public string HectaresFormatado => LimiteHectares >= 999999
            ? "Personalizado"
            : $"{LimiteHectares:N0} ha";
    }

    public class PlanoCreateDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descrição é obrigatória")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valor anual é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "Valor deve ser positivo")]
        public decimal ValorAnual { get; set; }

        [Required(ErrorMessage = "Limite de hectares é obrigatório")]
        [Range(1, double.MaxValue, ErrorMessage = "Limite de hectares deve ser maior que zero")]
        public decimal LimiteHectares { get; set; }

        public bool RequereContato { get; set; } = false;

        public int? EfiPayPlanIdInt { get; set; }
    }
}
