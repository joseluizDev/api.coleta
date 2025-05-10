namespace api.funcionario.Models.DTOs
{

    public class QueryFuncionario
    {
        public int? Page { get; set; }
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
    public class FuncionarioRequestDTO
   {
      public string Nome { get; set; }
      public string CPF { get; set; }
      public string Email { get; set; }
      public string Telefone { get; set; }
      public string Senha { get; set; }
      public string Observacao { get; set; }
      public bool Ativo { get; set; }
   }

   public class FuncionarioResponseDTO
   {
      public Guid Id { get; set; }
      public string Nome { get; set; }
      public string CPF { get; set; }
      public string Email { get; set; }
      public string Telefone { get; set; }
      public string Observacao { get; set; }
      public bool Ativo { get; set; }
   }
}
