
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.coleta.Models.Entidades;
using api.fazenda.Models.Entidades;


namespace api.safra.Models.Entidades
{
   public class Safra : Entity
   {
      [MaxLength(255)]
      public string Observacao { get; set; }

      public DateTime DataInicio { get; set; }

      public DateTime DataFim { get; set; }

      [ForeignKey("Fazenda")]
      public Guid FazendaID { get; set; }
      public virtual Fazenda Fazenda { get; set; }

      public Safra()
      {
      }

   }
}