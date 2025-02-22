using api.cliente.Models.DTOs;
using api.fazenda.models;

namespace api.safra.Models.DTOs
{
    public class SafraRequestDTO
    {
        public Guid? Id { get; set; }
        public string? Observacao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public Guid FazendaID { get; set; }
        public Guid ClienteID { get; set; }
    }

    public class SafraResponseDTO
    {
        public Guid Id { get; set; }
        public string? Observacao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public Guid FazendaID { get; set; }
        public FazendaResponseDTO Fazenda { get; set; }
        public Guid ClienteID { get; set; }
        public ClienteResponseDTO Cliente { get; set; }
    }
}
