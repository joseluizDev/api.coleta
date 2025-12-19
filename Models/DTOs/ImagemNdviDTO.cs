namespace api.coleta.Models.DTOs
{
    public class ImagemNdviUploadDTO
    {
        public IFormFile? Arquivo { get; set; }
        public DateTime DataImagem { get; set; }
        public Guid TalhaoId { get; set; }

        // Tipo da imagem: "ndvi" ou "altimetria"
        public string TipoImagem { get; set; } = "ndvi";

        // Campos NDVI (nullable para suportar altimetria)
        public double? PercentualNuvens { get; set; }
        public double? NdviMax { get; set; }
        public double? NdviMin { get; set; }

        // Campos Altimetria
        public double? AltimetriaMin { get; set; }
        public double? AltimetriaMax { get; set; }
        public double? AltimetriaVariacao { get; set; }
    }

    public class ImagemNdviOutputDTO
    {
        public Guid Id { get; set; }
        public string? LinkImagem { get; set; }
        public DateTime DataImagem { get; set; }
        public Guid TalhaoId { get; set; }
        public Guid FazendaId { get; set; }
        public DateTime DataInclusao { get; set; }

        // Tipo da imagem: "ndvi" ou "altimetria"
        public string TipoImagem { get; set; } = "ndvi";

        // Campos NDVI
        public double? PercentualNuvens { get; set; }
        public double? NdviMax { get; set; }
        public double? NdviMin { get; set; }

        // Campos Altimetria
        public double? AltimetriaMin { get; set; }
        public double? AltimetriaMax { get; set; }
        public double? AltimetriaVariacao { get; set; }
    }
}
