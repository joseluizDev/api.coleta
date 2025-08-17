using api.coleta.Data.Repository;
using api.coleta.Services;
using AutoMapper;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Text.Json;
using api.utils.DTOs;
using Microsoft.AspNetCore.Http.Features;
using LibTessDotNet;

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

            for (int row = 0; row < ((bounds.MaxY - bounds.MinY) / vertDist) + 1; row++)
            {
                for (int col = 0; col < ((bounds.MaxX - bounds.MinX) / hexWidth) + 1; col++)
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

            var features = new List<object>();

            for (int i = 0; i < hexagons.Count; i++)
            {
                var hex = hexagons[i];

                var coordinates = new[]
                {
                    hex.Coordinates
                    .Select(c => {
                    var transformed = transform.Transform(new[] { c.X, c.Y });
                    return new[] { transformed[0], transformed[1] };
                    })
                    .ToArray()
                };

                var feature = new
                {
                    type = "Feature",
                    properties = new { type = "hexagon", id = i + 1 },
                    geometry = new
                    {
                        type = "Polygon",
                        coordinates = coordinates
                    }
                };

                features.Add(feature);
            }

            var featureCollection = new
            {
                type = "FeatureCollection",
                features = features
            };

            var json = JsonSerializer.Serialize(featureCollection);
            return JsonSerializer.Deserialize<JsonElement>(json);

            //var geoJson = new
            //{
            //    type = "FeatureCollection",
            //    features = hexagons.Select(hex => new
            //    {
            //        type = "Feature",
            //        properties = new { type = "hexagon" },
            //        geometry = new
            //        {
            //            type = "Polygon",
            //            coordinates = new[] { hex.Coordinates.Select(c => new[]
            //    {
            //        transform.Transform(new[] { c.X, c.Y })[0],
            //        transform.Transform(new[] { c.X, c.Y })[1]
            //    }).ToArray() }
            //        }
            //    }).ToList()
            //};

            //return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(geoJson));
        }


        public PontosDentroDaAreaResponse GetPointsInsideArea(PontosDentroDaAreaRequest dados)
        {
            try
            {
                // Configurar seed para determinismo
                int seed = dados.Seed ?? Environment.TickCount;
                var random = new Random(seed);

                var transformToUtm = GetWgs84ToUtm();

                // Lista para armazenar pontos com seus respectivos IDs de hexágono
                var pointsWithHexagonId = new List<(Coordinate Point, int HexagonId)>();
                var perHexCounts = new Dictionary<int, int>();
                var metodosUsados = new List<string>();

                int pontosPorHex = dados.QtdPontosNaArea;

                // Obter a coleção de features original para acessar os IDs dos hexágonos
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(dados.GeoJsonAreas.ToString());

                // Processar cada feature (hexágono)
                for (int fIdx = 0; fIdx < featureCollection.Count; fIdx++)
                {
                    var feature = featureCollection[fIdx];
                    int hexagonId = GetHexagonId(featureCollection, fIdx);

                    List<Coordinate> pontosGerados = new();
                    string metodoUsado = "triangulacao";

                    if (feature.Geometry is Polygon poly)
                    {
                        // Transformar para UTM
                        var polyUtm = TransformPolygon(poly, transformToUtm);
                        pontosGerados = GenerateExactPointsForPolygon(polyUtm, pontosPorHex, random, out metodoUsado);
                    }
                    else if (feature.Geometry is MultiPolygon multi)
                    {
                        // Transformar cada componente para UTM e distribuir por área
                        var polysUtm = multi.Geometries.Cast<Polygon>().Select(p => TransformPolygon(p, transformToUtm)).ToList();
                        pontosGerados = DistributePointsInMultiPolygon(polysUtm, pontosPorHex, random);
                        metodoUsado = "triangulacao"; // pode incluir fallback internamente
                    }
                    else
                    {
                        // Geometria não suportada
                        continue;
                    }

                    // Adicionar os pontos com o ID do hexágono
                    foreach (var ponto in pontosGerados)
                    {
                        pointsWithHexagonId.Add((ponto, hexagonId));
                    }

                    // Registrar estatísticas
                    perHexCounts[hexagonId] = pontosGerados.Count;
                    if (!metodosUsados.Contains(metodoUsado))
                    {
                        metodosUsados.Add(metodoUsado);
                    }
                }

                // Criar resposta com metadados
                var pointsGeoJson = ConvertPointsToGeoJson(pointsWithHexagonId);
                var meta = new PontosDentroDaAreaMeta
                {
                    PerHexCounts = perHexCounts,
                    SeedUsado = seed,
                    Metodo = string.Join(", ", metodosUsados)
                };

                return new PontosDentroDaAreaResponse
                {
                    Points = pointsGeoJson,
                    Meta = meta
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar pontos dentro da área: " + ex.Message);
            }
        }

        private List<Polygon> ParseFeatureCollection(JsonElement geoJsonAreas)
        {
            var reader = new NetTopologySuite.IO.GeoJsonReader();
            var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(geoJsonAreas.ToString());

            var polygons = new List<Polygon>();
            foreach (var feature in featureCollection)
            {
                if (feature.Geometry is Polygon poly)
                {
                    polygons.Add(poly);
                }
                else if (feature.Geometry is MultiPolygon multi)
                {
                    polygons.AddRange(multi.Geometries.Cast<Polygon>());
                }
            }

            return polygons;
        }

        /// <summary>
        /// Obtém o ID do hexágono das propriedades do GeoJSON
        /// </summary>
        private int GetHexagonId(NetTopologySuite.Features.FeatureCollection featureCollection, int index)
        {
            if (index < featureCollection.Count)
            {
                var feature = featureCollection[index];
                if (feature.Attributes.Exists("id"))
                {
                    object idObj = feature.Attributes["id"];
                    if (idObj != null)
                    {
                        return Convert.ToInt32(idObj);
                    }
                }
            }
            return index + 1; // Fallback para índice + 1
        }

        /// <summary>
        /// Gera exatamente N pontos para um polígono usando triangulação com fallback
        /// </summary>
        private List<Coordinate> GenerateExactPointsForPolygon(Polygon polygon, int numPoints, Random random, out string metodoUsado)
        {
            var points = new List<Coordinate>();
            metodoUsado = "triangulacao";

            // Tentar triangulação primeiro
            var triangulationPoints = GeneratePointsByTriangulation(polygon, numPoints, random);

            if (triangulationPoints.Count >= numPoints)
            {
                // Triangulação foi suficiente, pegar exatamente N pontos
                points.AddRange(triangulationPoints.Take(numPoints));
            }
            else
            {
                // Triangulação parcial + fallback
                points.AddRange(triangulationPoints);
                int remainingPoints = numPoints - triangulationPoints.Count;

                if (remainingPoints > 0)
                {
                    var fallbackPoints = GeneratePointsByRejectionSampling(polygon, remainingPoints, random);
                    points.AddRange(fallbackPoints);
                    metodoUsado = triangulationPoints.Count > 0 ? "triangulacao+rejection_sampling" : "rejection_sampling";
                }
            }

            // Garantir que temos exatamente N pontos
            if (points.Count > numPoints)
            {
                points = points.Take(numPoints).ToList();
            }
            else if (points.Count < numPoints)
            {
                // Último recurso: duplicar pontos existentes com pequeno jitter
                var existingPoints = new List<Coordinate>(points);
                while (points.Count < numPoints && existingPoints.Count > 0)
                {
                    var basePoint = existingPoints[random.Next(existingPoints.Count)];
                    var jitteredPoint = new Coordinate(
                        basePoint.X + (random.NextDouble() - 0.5) * 0.1, // Jitter muito pequeno
                        basePoint.Y + (random.NextDouble() - 0.5) * 0.1
                    );
                    points.Add(jitteredPoint);
                }
                metodoUsado += "+jitter";
            }

            return points;
        }




        private JsonElement ConvertPointsToGeoJson(List<(Coordinate Point, int HexagonId)> pointsWithHexagonId)
        {
            var transform = GetUtmToWgs84();

            var features = new List<object>();
            int pointId = 1; // ID sequencial para os pontos

            foreach (var (point, hexagonId) in pointsWithHexagonId)
            {
                var transformedPoint = transform.Transform(new[] { point.X, point.Y });
                var feature = new
                {
                    type = "Feature",
                    properties = new
                    {
                        type = "point",
                        id = pointId,
                        hexagonId = hexagonId,
                        coletado = false
                    },
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { transformedPoint[0], transformedPoint[1] }
                    }
                };

                features.Add(feature);
                pointId++;
            }

            return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(features));
        }

        // Método original mantido para compatibilidade
        private JsonElement ConvertPointsToGeoJson(List<Coordinate> points)
        {
            var transform = GetUtmToWgs84();

            var features = points.Select((p, index) =>
            {
                var transformedPoint = transform.Transform(new[] { p.X, p.Y });
                return new
                {
                    type = "Feature",
                    properties = new
                    {
                        type = "point",
                        id = index + 1 // ID sequencial para os pontos
                    },
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { transformedPoint[0], transformedPoint[1] }
                    }
                };
            }).ToList();

            return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(features));
        }

        #region Triangulation Methods

        /// <summary>
        /// Gera pontos usando triangulação de polígonos
        /// </summary>
        private List<Coordinate> GeneratePointsByTriangulation(Polygon polygon, int numPoints, Random random)
        {
            try
            {
                var triangles = TriangulatePolygon(polygon);
                if (triangles == null || triangles.Count == 0)
                {
                    return new List<Coordinate>(); // Fallback será usado
                }

                return DistributePointsInTriangles(triangles, numPoints, random);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na triangulação: {ex.Message}");
                return new List<Coordinate>(); // Fallback será usado
            }
        }

        /// <summary>
        /// Triangula um polígono usando Poly2Tri
        /// </summary>
        private List<(Vec3 A, Vec3 B, Vec3 C)> TriangulatePolygon(Polygon polygon)
        {
            try
            {
                var tess = new Tess();

                // Adicionar contorno externo
                var outerRing = polygon.ExteriorRing.Coordinates;
                var outerVertices = outerRing.Take(outerRing.Length - 1)
                    .Select(c => new ContourVertex { Position = new Vec3((float)c.X, (float)c.Y, 0) })
                    .ToArray();
                tess.AddContour(outerVertices, ContourOrientation.Original);

                // Adicionar buracos
                for (int i = 0; i < polygon.NumInteriorRings; i++)
                {
                    var hole = polygon.GetInteriorRingN(i).Coordinates;
                    var holeVertices = hole.Take(hole.Length - 1)
                        .Select(c => new ContourVertex { Position = new Vec3((float)c.X, (float)c.Y, 0) })
                        .ToArray();
                    tess.AddContour(holeVertices, ContourOrientation.Original);
                }

                // Triangular
                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

                var triangles = new List<(Vec3 A, Vec3 B, Vec3 C)>();
                for (int i = 0; i < tess.ElementCount; i++)
                {
                    int idx0 = tess.Elements[i * 3 + 0];
                    int idx1 = tess.Elements[i * 3 + 1];
                    int idx2 = tess.Elements[i * 3 + 2];
                    if (idx0 == -1 || idx1 == -1 || idx2 == -1) continue; // skip degenerate
                    var a = tess.Vertices[idx0].Position;
                    var b = tess.Vertices[idx1].Position;
                    var c = tess.Vertices[idx2].Position;
                    triangles.Add((a, b, c));
                }

                return triangles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na triangulação do polígono: {ex.Message}");
                return new List<(Vec3, Vec3, Vec3)>();
            }
        }

        /// <summary>
        /// Distribui pontos entre triângulos proporcionalmente às suas áreas
        /// </summary>
        private List<Coordinate> DistributePointsInTriangles(List<(Vec3 A, Vec3 B, Vec3 C)> triangles, int numPoints, Random random)
        {
            var points = new List<Coordinate>();

            // Calcular áreas dos triângulos
            var triangleAreas = triangles.Select(CalculateTriangleArea).ToList();
            var totalArea = triangleAreas.Sum();

            if (totalArea <= 0)
            {
                return points;
            }

            // Distribuir pontos proporcionalmente
            var pointsDistributed = 0;
            for (int i = 0; i < triangles.Count && pointsDistributed < numPoints; i++)
            {
                var triangle = triangles[i];
                var area = triangleAreas[i];

                // Calcular quantos pontos este triângulo deve receber
                int pointsForTriangle;
                if (i == triangles.Count - 1) // Último triângulo recebe os pontos restantes
                {
                    pointsForTriangle = numPoints - pointsDistributed;
                }
                else
                {
                    pointsForTriangle = (int)Math.Round((area / totalArea) * numPoints);
                }

                // Gerar pontos no triângulo
                for (int j = 0; j < pointsForTriangle; j++)
                {
                    var point = GenerateRandomPointInTriangle(triangle.A, triangle.B, triangle.C, random);
                    points.Add(point);
                    pointsDistributed++;
                }
            }

            return points;
        }

        /// <summary>
        /// Calcula a área de um triângulo
        /// </summary>
        private double CalculateTriangleArea((Vec3 A, Vec3 B, Vec3 C) triangle)
        {
            var p1 = triangle.A;
            var p2 = triangle.B;
            var p3 = triangle.C;

            return Math.Abs((p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y)) / 2.0);
        }

        /// <summary>
        /// Gera um ponto aleatório dentro de um triângulo usando coordenadas baricêntricas
        /// </summary>
        private Coordinate GenerateRandomPointInTriangle(Vec3 p1, Vec3 p2, Vec3 p3, Random random)
        {
            // Gerar coordenadas baricêntricas aleatórias
            double u = random.NextDouble();
            double v = random.NextDouble();

            // Garantir que u + v <= 1
            if (u + v > 1)
            {
                u = 1 - u;
                v = 1 - v;
            }

            double w = 1 - u - v;

            // Converter para coordenadas cartesianas
            double x = u * p1.X + v * p2.X + w * p3.X;
            double y = u * p1.Y + v * p2.Y + w * p3.Y;

            return new Coordinate(x, y);
        }

        /// <summary>
        /// Distribui pontos entre componentes de MultiPolygon proporcionalmente às suas áreas
        /// </summary>
        private List<Coordinate> DistributePointsInMultiPolygon(List<Polygon> polygons, int numPoints, Random random)
        {
            var allPoints = new List<Coordinate>();

            // Calcular áreas dos polígonos
            var polygonAreas = polygons.Select(p => p.Area).ToList();
            var totalArea = polygonAreas.Sum();

            if (totalArea <= 0)
            {
                return allPoints;
            }

            // Distribuir pontos proporcionalmente
            var pointsDistributed = 0;
            for (int i = 0; i < polygons.Count && pointsDistributed < numPoints; i++)
            {
                var polygon = polygons[i];
                var area = polygonAreas[i];

                // Calcular quantos pontos este polígono deve receber
                int pointsForPolygon;
                if (i == polygons.Count - 1) // Último polígono recebe os pontos restantes
                {
                    pointsForPolygon = numPoints - pointsDistributed;
                }
                else
                {
                    pointsForPolygon = (int)Math.Round((area / totalArea) * numPoints);
                }

                // Gerar pontos no polígono usando triangulação
                var polygonPoints = GeneratePointsByTriangulation(polygon, pointsForPolygon, random);

                // Se triangulação falhou, usar fallback
                if (polygonPoints.Count < pointsForPolygon)
                {
                    var fallbackPoints = GeneratePointsByRejectionSampling(polygon, pointsForPolygon - polygonPoints.Count, random);
                    polygonPoints.AddRange(fallbackPoints);
                }

                allPoints.AddRange(polygonPoints);
                pointsDistributed += polygonPoints.Count;
            }

            return allPoints;
        }

        /// <summary>
        /// Fallback: Gera pontos usando rejection sampling no bbox do polígono
        /// </summary>
        private List<Coordinate> GeneratePointsByRejectionSampling(Polygon polygon, int numPoints, Random random, int maxAttempts = 10000)
        {
            var points = new List<Coordinate>();
            var bounds = polygon.EnvelopeInternal;
            var preparedPolygon = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(polygon);

            int attempts = 0;
            int pointsGenerated = 0;

            while (pointsGenerated < numPoints && attempts < maxAttempts)
            {
                // Gerar ponto aleatório no bbox
                double x = bounds.MinX + random.NextDouble() * (bounds.MaxX - bounds.MinX);
                double y = bounds.MinY + random.NextDouble() * (bounds.MaxY - bounds.MinY);

                var point = new Point(x, y) { SRID = polygon.SRID };

                // Verificar se está dentro do polígono
                if (preparedPolygon.Contains(point))
                {
                    points.Add(new Coordinate(x, y));
                    pointsGenerated++;
                }

                attempts++;
            }

            // Se não conseguiu gerar pontos suficientes, usar fallback determinístico
            if (pointsGenerated < numPoints)
            {
                var remainingPoints = GenerateDeterministicFallbackPoints(polygon, numPoints - pointsGenerated, random);
                points.AddRange(remainingPoints);
            }

            return points;
        }

        /// <summary>
        /// Fallback determinístico: usa centroide com jitter aleatório
        /// </summary>
        private List<Coordinate> GenerateDeterministicFallbackPoints(Polygon polygon, int numPoints, Random random)
        {
            var points = new List<Coordinate>();
            var centroid = polygon.Centroid;
            var bounds = polygon.EnvelopeInternal;
            var preparedPolygon = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(polygon);

            // Calcular raio máximo para jitter (10% da menor dimensão do bbox)
            double maxJitter = Math.Min(bounds.Width, bounds.Height) * 0.1;

            for (int i = 0; i < numPoints; i++)
            {
                Coordinate point = null;
                int attempts = 0;
                const int maxJitterAttempts = 100;

                // Tentar gerar ponto próximo ao centroide
                while (point == null && attempts < maxJitterAttempts)
                {
                    double jitterX = (random.NextDouble() - 0.5) * 2 * maxJitter;
                    double jitterY = (random.NextDouble() - 0.5) * 2 * maxJitter;

                    double x = centroid.X + jitterX;
                    double y = centroid.Y + jitterY;

                    var testPoint = new Point(x, y) { SRID = polygon.SRID };

                    if (preparedPolygon.Contains(testPoint))
                    {
                        point = new Coordinate(x, y);
                    }

                    attempts++;
                }

                // Se ainda não conseguiu, usar o centroide mesmo
                if (point == null)
                {
                    point = new Coordinate(centroid.X, centroid.Y);
                }

                points.Add(point);
            }

            return points;
        }

        #endregion


    }
}
