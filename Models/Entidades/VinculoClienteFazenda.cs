
using System.ComponentModel.DataAnnotations.Schema;
using api.cliente.Models.Entidades;
using api.fazenda.Models.Entidades;


namespace
 api.VinculoClienteFazenda.Models.Entidades
{
   public class VinculoClienteFazenda : Entity
   {
      [ForeignKey("Cliente")]
      public Guid ClienteId { get; set; }
      public Cliente Cliente { get; set; }

      [ForeignKey("Fazenda")]
      public Guid FazendaId { get; set; }
      public Fazenda Fazenda { get; set; }
      public bool Ativo { get; set; }

      public VinculoClienteFazenda()
      {
      }
   }
}