using System.ComponentModel.DataAnnotations.Schema;
using api.fazenda.Models.Entidades;

namespace api.coleta.Models.Entidades
{
    public class ImagemNdvi : Entity
    {
    public string? LinkImagem { get; set; }
        public DateTime DataImagem { get; set; }
        // Percentual de nuvens (0-100)
        public double PercentualNuvens { get; set; }
        public double NdviMax { get; set; }
        public double NdviMin { get; set; }

        public Guid TalhaoId { get; set; }
        public Guid FazendaId { get; set; }
        public Guid UsuarioId { get; set; }

        // Navegações simples (não obrigatórias para salvar mas ajudam EF se precisar incluir depois)
    public virtual Talhao? Talhao { get; set; }
    public virtual Fazenda? Fazenda { get; set; }
    public virtual Usuario? Usuario { get; set; }
    }
}
