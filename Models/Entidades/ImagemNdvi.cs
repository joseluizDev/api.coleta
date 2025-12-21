using System.ComponentModel.DataAnnotations.Schema;
using api.fazenda.Models.Entidades;

namespace api.coleta.Models.Entidades
{
    public class ImagemNdvi : Entity
    {
        public string? LinkImagem { get; set; }
        public DateTime DataImagem { get; set; }

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

        public Guid TalhaoId { get; set; }
        public Guid FazendaId { get; set; }
        public Guid UsuarioId { get; set; }

        // Navegações simples (não obrigatórias para salvar mas ajudam EF se precisar incluir depois)
        public virtual Talhao? Talhao { get; set; }
        public virtual Fazenda? Fazenda { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
