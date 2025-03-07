using api.cliente.Models.DTOs;
using api.fazenda.models;

namespace api.talhao.Models.DTOs
{
    public class TalhaoRequestDTO
    {
        public Guid? Id { get; set; }
        public string Nome { get; set; }
        public string Cultura { get; set; }
        public string Variedade { get; set; }
        public string Observacao { get; set; }
        public double Area { get; set; }
        public string LinkGeoJson { get; set; }
        public Guid FazendaID { get; set; }
        public Guid ClienteID { get; set; }
    }

    public class TalhaoResponseDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Cultura { get; set; }
        public string Variedade { get; set; }
        public string Observacao { get; set; }
        public double Area { get; set; }
        public string LinkGeoJson { get; set; }
        public Guid FazendaID { get; set; }
        public FazendaResponseDTO Fazenda { get; set; }
        public Guid ClienteID { get; set; }
        public ClienteResponseDTO Cliente { get; set; }
    }
}
