namespace api.coleta.Models.DTOs.Licenciamento
{
    /// <summary>
    /// DTO para mapear dados de planos do Gateway de Pagamentos (PostgreSQL).
    /// Planos são gerenciados exclusivamente pelo gateway Python - este DTO
    /// é usado apenas para deserializar respostas da API do gateway.
    /// </summary>
    public class PlanoDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorAnual { get; set; }
        public decimal LimiteHectares { get; set; }
        public bool Ativo { get; set; }
        public bool RequereContato { get; set; }

        public string ValorFormatado => ValorAnual > 0
            ? ValorAnual.ToString("C", new System.Globalization.CultureInfo("pt-BR"))
            : "Sob consulta";

        public string HectaresFormatado => LimiteHectares >= 999999
            ? "Personalizado"
            : $"{LimiteHectares:N0} ha";
    }
}
