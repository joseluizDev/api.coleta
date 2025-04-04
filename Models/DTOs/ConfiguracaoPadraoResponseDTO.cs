namespace api.coleta.Models.DTOs
{
    public class ConfiguracaoPadraoResponseDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public decimal Limite { get; set; }
        public string CorHex { get; set; }
    }
}
