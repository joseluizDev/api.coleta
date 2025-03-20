using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.coleta.Models.Entidades;
using api.fazenda.Models.Entidades;


namespace api.coleta.Models.Entidades
{
    public class TalhaoJson : Entity
    {
        public Guid TalhaoID { get; set; }
        public string Area { get; set; }
        public virtual Talhao Talhao { get; set; }
        [Column(TypeName = "JSON")]
        public string Coordenadas { get; set; }
        [MaxLength(255)]
        public string Nome { get; set; }
        [MaxLength(255)]
        public string? Observacao { get; set; }
        public TalhaoJson()
        {
        }
    }
}