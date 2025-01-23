
using System.ComponentModel.DataAnnotations.Schema;
using api.fazenda.Models.Entidades;

namespace api.coleta.Models.Entidades
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