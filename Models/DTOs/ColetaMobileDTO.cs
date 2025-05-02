using System.Text.Json;

namespace api.coleta.Models.DTOs
{
    public class ColetaMobileDTO
    {
        public Guid ColetaID { get; set; }
        public Guid? FuncionarioID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DataColeta { get; set; }
        public PontoDto Ponto { get; set; }
    }

    public class PontoDto
    {
        public int Id { get; set; }
        public int HexagonId { get; set; }
        public bool Coletado { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class PropertiesDto
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public int HexagonId { get; set; }
        public bool Coletado { get; set; }
    }

    public class GeometryDto
    {
        public string Type { get; set; }
        public JsonElement Coordinates { get; set; }
    }
}
