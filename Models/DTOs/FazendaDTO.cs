
using api.cliente.Models.DTOs;

namespace api.fazenda.models
{
    public class QueryFazenda
    {
        public int? Page { get; set; }
        public string? Nome { get; set; }
        public Guid? ClienteID { get; set; }
    }
    public class FazendaRequestDTO
    {
        public Guid? Id { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Guid ClienteID { get; set; }
    }

    public class FazendaResponseDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Guid ClienteID { get; set; }
    }
}
