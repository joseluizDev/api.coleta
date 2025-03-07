
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.fazenda.Models.Entidades;


namespace api.coleta.Models.Entidades
{
    public class Safra : Entity
    {
        [MaxLength(255)]
        public string? Observacao { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; } = null;

        [ForeignKey("Fazenda")]
        public Guid FazendaID { get; set; }
        public virtual Fazenda Fazenda { get; set; }

        public Guid ClienteID { get; set; }
        public virtual Cliente Cliente { get; set; }

        public Guid UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }

        public Safra()
        {
        }

    }
}