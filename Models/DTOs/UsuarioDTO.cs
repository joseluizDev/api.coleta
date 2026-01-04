namespace api.coleta.Models.DTOs
{
    public class UsuarioResquestDTO
    {
        public string NomeCompleto { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Senha { get; set; }
        public Guid adminId { get; set; }
        public string? FcmToken { get; set; }

        // Campos de endereço para criar cliente automaticamente
        public string? Cep { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }

    public class UsuarioResponseDTO
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string? FcmToken { get; set; }
    }

    public class UsuarioLoginDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}
