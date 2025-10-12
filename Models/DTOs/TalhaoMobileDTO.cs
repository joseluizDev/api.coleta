namespace api.talhao.Models.DTOs
{
    public class CoordenadasDTO
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class TalhaoMobileRequestDTO
    {
        public Guid FazendaID { get; set; }
        public required string Nome { get; set; }
        public double Area { get; set; }
        public string? Observacao { get; set; }
        public required List<CoordenadasDTO> Coordenadas { get; set; }
    }
}
