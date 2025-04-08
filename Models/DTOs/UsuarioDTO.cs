namespace api.coleta.Models.DTOs
{
    public class UsuarioResquestDTO
    {
        public string NomeCompleto { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Senha { get; set; }
    }

    public class UsuarioResponseDTO
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
    }

    public class UsuarioLoginDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}
