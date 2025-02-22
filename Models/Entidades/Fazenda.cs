using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.coleta.Models.Entidades;

namespace api.fazenda.Models.Entidades
{
   public class Fazenda : Entity
   {
      [MaxLength]
      public string Nome { get; set; }
      [MaxLength(255)]
      public string Endereco { get; set; }
      [MaxLength(12)]
      public double Lat { get; set; }
      [MaxLength(12)]
      public double Lng { get; set; }

      [ForeignKey("Cliente")]
      public Guid ClienteID { get; set; }
      public virtual Cliente Cliente { get; set; }
      public Guid UsuarioID { get; set; }
      public virtual Usuario Usuario { get; set; }
      public Fazenda()
      {
      }
   }
}