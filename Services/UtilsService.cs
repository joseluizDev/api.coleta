using api.coleta.Data.Repository;
using api.coleta.Services;
using AutoMapper;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Text.Json;
using api.utils.DTOs;

namespace api.vinculoClienteFazenda.Services
{
   public class UtilsService : ServiceBase
   {
      private readonly GeometryFactory _geometryFactory = new();
      private readonly CoordinateTransformationFactory _ctFactory = new();
      private readonly CoordinateSystemFactory _csFactory = new();
      private readonly VinculoClienteFazendaRepository _vinculoRepository;

      public UtilsService(VinculoClienteFazendaRepository vinculoRepository, IUnitOfWork unitOfWork, IMapper mapper)
          : base(unitOfWork, mapper)
      {
         _vinculoRepository = vinculoRepository;
      }

      public JsonElement GenerateHexagons(JsonElement polygonGeoJson, double hectares)
      {
         try
         {
            var inputPolygon = ParsePolygon(polygonGeoJson);
            var transformedPolygon = TransformPolygon(inputPolygon, GetWgs84ToUtm());
            var hexagons = GenerateHexagonalGrid(transformedPolygon, hectares);
            var geoJson = ConvertHexagonsToGeoJson(hexagons);
            return geoJson;

         }
         catch (Exception ex)
         {
            throw new Exception("Erro ao gerar hexágonos: " + ex.Message);
         }
      }

      private Polygon ParsePolygon(JsonElement polygonGeoJson)
      {
         JsonElement coordsElement = polygonGeoJson.TryGetProperty("features", out var features) && features.GetArrayLength() > 0
             ? features[0].GetProperty("geometry").GetProperty("coordinates")
             : polygonGeoJson.GetProperty("coordinates");

         var coordinates = coordsElement[0].EnumerateArray()
             .Select(coord => new Coordinate(coord[0].GetDouble(), coord[1].GetDouble()))
             .ToList();

         if (!coordinates.First().Equals2D(coordinates.Last()))
         {
            coordinates.Add(coordinates.First());
         }

         return _geometryFactory.CreatePolygon(coordinates.ToArray());
      }

      private MathTransform GetWgs84ToUtm() => _ctFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WGS84_UTM(23, true)).MathTransform;
      private MathTransform GetUtmToWgs84() => _ctFactory.CreateFromCoordinateSystems(ProjectedCoordinateSystem.WGS84_UTM(23, true), GeographicCoordinateSystem.WGS84).MathTransform;

      private Polygon TransformPolygon(Polygon polygon, MathTransform transform)
      {
         var transformedCoords = polygon.Coordinates.Select(c => new Coordinate(transform.Transform(new[] { c.X, c.Y })[0], transform.Transform(new[] { c.X, c.Y })[1])).ToArray();
         return _geometryFactory.CreatePolygon(transformedCoords);
      }

      private List<Geometry> GenerateHexagonalGrid(Polygon projectedPolygon, double hectares)
      {
         double areaM2 = hectares * 10000;
         double r = Math.Sqrt((2 * areaM2) / (3 * Math.Sqrt(3)));
         double hexWidth = Math.Sqrt(3) * r;
         double hexHeight = 2 * r;
         double vertDist = hexHeight * 0.75;
         var bounds = projectedPolygon.EnvelopeInternal;
         var hexagons = new List<Geometry>();

         for (int row = 0; row < ((bounds.MaxY - bounds.MinY) / vertDist) + 3; row++)
         {
            for (int col = 0; col < ((bounds.MaxX - bounds.MinX) / hexWidth) + 3; col++)
            {
               double offset = (row % 2 == 0) ? 0 : hexWidth / 2;
               double centerX = bounds.MinX + col * hexWidth + offset;
               double centerY = bounds.MinY + row * vertDist;
               Polygon hexagon = CreateHexagon(new Coordinate(centerX, centerY), r);

               if (projectedPolygon.Intersects(hexagon))
               {
                  var intersection = projectedPolygon.Intersection(hexagon);
                  if (!intersection.IsEmpty)
                  {
                     hexagons.Add(intersection);
                  }
               }
            }
         }
         return hexagons;
      }

      private Polygon CreateHexagon(Coordinate center, double r)
      {
         var vertices = Enumerable.Range(0, 6)
             .Select(i => new Coordinate(center.X + r * Math.Cos(Math.PI / 180 * (60 * i - 30)), center.Y + r * Math.Sin(Math.PI / 180 * (60 * i - 30))))
             .ToList();
         vertices.Add(vertices.First());
         return _geometryFactory.CreatePolygon(vertices.ToArray());
      }

      private JsonElement ConvertHexagonsToGeoJson(List<Geometry> hexagons)
      {
         var transform = GetUtmToWgs84();
         var geoJson = new
         {
            type = "FeatureCollection",
            features = hexagons.Select(hex => new
            {
               type = "Feature",
               properties = new { type = "hexagon" },
               geometry = new
               {
                  type = "Polygon",
                  coordinates = new[] { hex.Coordinates.Select(c => new[]
                {
                    transform.Transform(new[] { c.X, c.Y })[0],
                    transform.Transform(new[] { c.X, c.Y })[1]
                }).ToArray() }
               }
            }).ToList()
         };

         return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(geoJson));
      }


      public JsonElement GetPointsInsideArea(PontosDentroDaAreaRequest dados)
      {
         var inputPolygon = ParsePolygon(dados.Polygon);
         var preparedPolygon = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(inputPolygon);
         var bounds = inputPolygon.EnvelopeInternal;
         var points = new List<Coordinate>();
         var random = new Random();
         int attempts = 0, maxAttempts = dados.QtdPontosNaArea * 20; // Aumenta tentativas para melhorar a precisão

         while (points.Count < dados.QtdPontosNaArea && attempts < maxAttempts)
         {
            double x = bounds.MinX + (bounds.MaxX - bounds.MinX) * random.NextDouble();
            double y = bounds.MinY + (bounds.MaxY - bounds.MinY) * random.NextDouble();

            // Criando ponto com o mesmo sistema de coordenadas do polígono
            var point = new Point(x, y) { SRID = inputPolygon.SRID };

            // Verifica se o ponto está dentro do polígono
            if (preparedPolygon.Contains(point))
            {
               points.Add(new Coordinate(x, y));
            }
            attempts++;
         }

         return ConvertPointsToGeoJson(points);
      }

      private JsonElement ConvertPointsToGeoJson(List<Coordinate> points)
      {
         var transform = GetUtmToWgs84();

         var features = points.Select(p =>
         {
            var transformedPoint = transform.Transform(new[] { p.X, p.Y });
            return new
            {
               type = "Feature",
               properties = new { type = "point" },
               geometry = new
               {
                  type = "Point",
                  coordinates = new[] { transformedPoint[0], transformedPoint[1] }
               }
            };
         }).ToList();

         return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(features));
      }


   }
}
