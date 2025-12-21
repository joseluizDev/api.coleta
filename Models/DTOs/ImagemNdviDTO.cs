namespace api.coleta.Models.DTOs
{
    public class ImagemNdviUploadDTO
    {
        public IFormFile? Arquivo { get; set; }
        public DateTime DataImagem { get; set; }
        public Guid TalhaoId { get; set; }

        // Tipo da imagem: "ndvi", "altimetria" ou "colheita"
        public string TipoImagem { get; set; } = "ndvi";

        // Campos NDVI (nullable para suportar outros tipos)
        public double? PercentualNuvens { get; set; }
        public double? NdviMax { get; set; }
        public double? NdviMin { get; set; }

        // Campos Altimetria
        public double? AltimetriaMin { get; set; }
        public double? AltimetriaMax { get; set; }
        public double? AltimetriaVariacao { get; set; }

        // Campos Mapa de Colheita
        public DateTime? DataImagemColheita { get; set; }
        public double? ColheitaMin { get; set; }
        public double? ColheitaMax { get; set; }
        public double? ColheitaMedia { get; set; }
    }

    public class ImagemNdviOutputDTO
    {
        public Guid Id { get; set; }
        public string? LinkImagem { get; set; }
        public DateTime DataImagem { get; set; }
        public Guid TalhaoId { get; set; }
        public Guid FazendaId { get; set; }
        public DateTime DataInclusao { get; set; }

        // Tipo da imagem: "ndvi", "altimetria" ou "colheita"
        public string TipoImagem { get; set; } = "ndvi";

        // Campos NDVI
        public double? PercentualNuvens { get; set; }
        public double? NdviMax { get; set; }
        public double? NdviMin { get; set; }

        // Campos Altimetria
        public double? AltimetriaMin { get; set; }
        public double? AltimetriaMax { get; set; }
        public double? AltimetriaVariacao { get; set; }

        // Campos Mapa de Colheita
        public DateTime? DataImagemColheita { get; set; }
        public double? ColheitaMin { get; set; }
        public double? ColheitaMax { get; set; }
        public double? ColheitaMedia { get; set; }
    }
}
