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
   }

}
