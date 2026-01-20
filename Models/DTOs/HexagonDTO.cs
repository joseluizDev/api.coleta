using System.Text.Json;

namespace api.utils.DTOs
{
   public class HexagonRequestDto
   {
      public JsonElement Polygon { get; set; }
      public double Hectares { get; set; }
   }
   public class HexagonResponseDto
   {
      public object Hexagonal { get; set; }
   }

   public class PontosDentroDaAreaRequest
   {
      public JsonElement GeoJsonAreas { get; set; } // FeatureCollection
      public int QtdPontosNaArea { get; set; }
      public int? Seed { get; set; } // Seed opcional para determinismo
   }

   public class PontosDentroDaAreaResponse
   {
      public JsonElement Points { get; set; } // Array de pontos GeoJSON
      public PontosDentroDaAreaMeta Meta { get; set; }
   }

   public class PontosDentroDaAreaMeta
   {
      public Dictionary<int, int> PerHexCounts { get; set; } // ID do hexágono -> quantidade de pontos gerados
      public int SeedUsado { get; set; } // Seed usado na geração
      public string Metodo { get; set; } // "triangulacao" ou "rejection_sampling"
   }

}
