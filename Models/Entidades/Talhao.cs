using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.coleta.Models.Entidades;
using api.fazenda.Models.Entidades;


namespace api.coleta.Models.Entidades
{
    public class Talhao : Entity
    {
        public Guid FazendaID { get; set; }

        public virtual Fazenda Fazenda { get; set; }

        public Guid ClienteID { get; set; }

        public virtual Cliente Cliente { get; set; }
        public Guid UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }

        public Talhao()
        {
        }
    }
}