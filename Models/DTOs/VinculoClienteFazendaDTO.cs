namespace api.VinculoClienteFazenda.Models.DTOs
{
   public class VinculoRequestDTO
   {
      public Guid ClienteId { get; set; }
      public Guid FazendaId { get; set; }
      public bool Ativo { get; set; }
   }

   public class VinculoResponseDTO
   {
      public Guid Id { get; set; }
      public Guid ClienteId { get; set; }
      public Guid FazendaId { get; set; }
      public bool Ativo { get; set; }
   }
}
