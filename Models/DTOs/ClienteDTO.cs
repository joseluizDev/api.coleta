


namespace api.cliente.Models.DTOs
{

    public class QueryClienteDTO
    {
        public int? Page { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
    public class ClienteRequestDTO
    {
        public Guid? Id { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }
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
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Cep { get; set; }
        public string Endereco { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
    }

    public class ClienteResponsePaginacaoDto
    {
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public int Total { get; set; }
        public int TotalPaginas { get; set; }
        public bool ProximaPagina { get; set; }
        public bool PaginaAnterior { get; set; }
        public List<ClienteResponseDTO> Clientes { get; set; }

    }
}
