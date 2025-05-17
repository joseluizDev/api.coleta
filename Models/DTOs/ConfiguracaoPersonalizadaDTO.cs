namespace api.coleta.Models.DTOs
{
    public class ConfiguracaoPersonalizadaRequestDTO
    {
        public string Nome { get; set; }
        public decimal LimiteInferior { get; set; }
        public decimal LimiteSuperior { get; set; }
        public string CorHex { get; set; }
    }

    public class ConfiguracaoPersonalizadaResponseDTO
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nome { get; set; }
        public decimal LimiteInferior { get; set; }
        public decimal LimiteSuperior { get; set; }
        public string CorHex { get; set; }
    }
}
