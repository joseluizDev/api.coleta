namespace api.safra.Models.DTOs
{
   public class SafraRequestDTO
   {
      public string Observacao { get; set; }
      public DateTime DataInicio { get; set; }
      public DateTime DataFim { get; set; }
      public Guid FazendaID { get; set; }
   }

   public class SafraResponseDTO
   {
      public Guid Id { get; set; }
      public string Observacao { get; set; }
      public DateTime DataInicio { get; set; }
      public DateTime DataFim { get; set; }
      public Guid FazendaID { get; set; }
   }
}
