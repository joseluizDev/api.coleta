namespace api.coleta.Models.DTOs
{
    public class ImagemNdviUploadDTO
    {
        public IFormFile? Arquivo { get; set; }
        public DateTime DataImagem { get; set; }
        public double PercentualNuvens { get; set; }
        public double NdviMax { get; set; }
        public double NdviMin { get; set; }
        public Guid TalhaoId { get; set; }
    }

    public class ImagemNdviOutputDTO
    {
        public Guid Id { get; set; }
        public string? LinkImagem { get; set; }
        public DateTime DataImagem { get; set; }
        public double PercentualNuvens { get; set; }
        public double NdviMax { get; set; }
        public double NdviMin { get; set; }
        public Guid TalhaoId { get; set; }
        public Guid FazendaId { get; set; }
        public DateTime DataInclusao { get; set; }
    }
}
