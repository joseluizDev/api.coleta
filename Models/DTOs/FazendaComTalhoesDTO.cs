using System.Collections.Generic;

namespace api.coleta.Models.DTOs
{
    public class FazendaComTalhoesDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Guid ClienteID { get; set; }
        public List<TalhaoMobileDTO> Talhoes { get; set; }

        public FazendaComTalhoesDTO()
        {
            Talhoes = new List<TalhaoMobileDTO>();
        }
    }

    public class TalhaoMobileDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Area { get; set; }
        public string Coordenadas { get; set; }
        public string? Observacao { get; set; }
        public Guid TalhaoID { get; set; }
        public Guid FazendaID { get; set; }
        public Guid ClienteID { get; set; }
    }
}