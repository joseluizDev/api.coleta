
namespace api.fazenda.models
{
   public class FazendaRequestDTO
   {
      public string Nome { get; set; }
      public string Endereco { get; set; }
      public double Lat { get; set; }
      public double Lng { get; set; }
      public Guid UsuarioID { get; set; }
   }

   public class FazendaResponseDTO
   {
      public Guid Id { get; set; }
      public string Nome { get; set; }
      public string Endereco { get; set; }
      public double Lat { get; set; }
      public double Lng { get; set; }
   }
}

