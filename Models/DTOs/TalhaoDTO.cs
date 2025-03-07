using api.cliente.Models.DTOs;
using api.fazenda.models;
using System.Collections.Generic;

namespace api.talhao.Models.DTOs
{
    public class TalhaoRequestDTO
    {
        public Guid? Id { get; set; }
        public Guid FazendaID { get; set; }
        public Guid ClienteID { get; set; }
        public List<Talhoes> Talhoes { get; set; }
    }

    public class TalhaoResponseDTO
    {
        public Guid Id { get; set; }
        public Guid FazendaID { get; set; }
        public FazendaResponseDTO Fazenda { get; set; }
        public ClienteResponseDTO Cliente { get; set; }
        public Guid ClienteID { get; set; }
        public List<Talhoes> Talhoes { get; set; }
    }

    public class Talhoes
    {
        public double Area { get; set; }
        public List<Coordenada> Coordenadas { get; set; }
    }

    public class Coordenada
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
