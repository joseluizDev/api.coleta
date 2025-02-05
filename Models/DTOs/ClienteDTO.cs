namespace api.cliente.Models.DTOs
{
   public class ClienteRequestDTO
   {
      public string Nome { get; set; }
      public string CPF { get; set; }
      public string Email { get; set; }
      public string Telefone { get; set; }
      public string Cep { get; set; }
      public string Endereco { get; set; }
      public string Cidade { get; set; }
      public string Estado { get; set; }
   }

   public class ClienteResponseDTO
   {
      public Guid Id { get; set; }
      public string Nome { get; set; }
      public string CPF { get; set; }
      public string Email { get; set; }
      public string Telefone { get; set; }
      public string Cep { get; set; }
      public string Endereco { get; set; }
      public string Cidade { get; set; }
      public string Estado { get; set; }
   }
}
