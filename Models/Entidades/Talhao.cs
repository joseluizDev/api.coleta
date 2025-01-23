using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.coleta.Models.Entidades;
using api.fazenda.Models.Entidades;


namespace api.talhao.Models.Entidades
{
   public class Talhao : Entity
   {
      [MaxLength(100)]
      public string Nome { get; set; }

      [MaxLength(100)]
      public string Cultura { get; set; }

      [MaxLength(100)]
      public string Variedade { get; set; }

      [MaxLength(255)]
      public string Observacao { get; set; }

      [MaxLength(20)]
      public double Area { get; set; }

      [MaxLength(255)]
      public string LinkGeoJson { get; set; }

      [ForeignKey("Fazenda")]
      public Guid FazendaID { get; set; }
      public virtual Fazenda Fazenda { get; set; }

      public Talhao()
      {
      }
   }
}