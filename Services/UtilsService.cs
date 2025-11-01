using api.coleta.Data.Repository;
using api.coleta.Services;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Text.Json;
using api.utils.DTOs;
using Microsoft.AspNetCore.Http.Features;
using LibTessDotNet;
using NetTopologySuite.Triangulate;

namespace api.vinculoClienteFazenda.Services
{
    public class UtilsService : ServiceBase
    {
        private readonly GeometryFactory _geometryFactory = new();
        private readonly CoordinateTransformationFactory _ctFactory = new();
        private readonly CoordinateSystemFactory _csFactory = new();
        private readonly VinculoClienteFazendaRepository _vinculoRepository;

        public UtilsService(VinculoClienteFazendaRepository vinculoRepository, IUnitOfWork unitOfWork)
            : base(unitOfWork)
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

            // Validar e corrigir o polígono de entrada
            var validatedPolygon = ValidateAndFixGeometry(projectedPolygon);
            if (validatedPolygon == null || validatedPolygon.IsEmpty)
            {
                throw new Exception("Polígono inválido após validação");
            }

            // Criar geometria preparada para melhor performance
            var preparedPolygon = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(validatedPolygon);

            for (int row = 0; row < ((bounds.MaxY - bounds.MinY) / vertDist) + 1; row++)
            {
                for (int col = 0; col < ((bounds.MaxX - bounds.MinX) / hexWidth) + 1; col++)
                {
                    double offset = (row % 2 == 0) ? 0 : hexWidth / 2;
                    double centerX = bounds.MinX + col * hexWidth + offset;
                    double centerY = bounds.MinY + row * vertDist;
                    Polygon hexagon = CreateHexagon(new Coordinate(centerX, centerY), r);

                    if (preparedPolygon.Intersects(hexagon))
                    {
                        try
                        {
                            // Validar e corrigir o hexágono antes da interseção
                            var validatedHexagon = ValidateAndFixGeometry(hexagon);
                            if (validatedHexagon != null && !validatedHexagon.IsEmpty)
                            {
                                var intersection = validatedPolygon.Intersection(validatedHexagon);
                                if (intersection != null && !intersection.IsEmpty && intersection.Area > 0)
                                {
                                    hexagons.Add(intersection);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Ignora hexágonos problemáticos e continua
                            Console.WriteLine($"Erro ao processar hexágono na posição ({row}, {col}): {ex.Message}");
                            continue;
                        }
                    }
                }
            }
            return hexagons;
        }

        /// <summary>
        /// Valida e corrige geometrias para evitar problemas topológicos
        /// </summary>
        private Geometry? ValidateAndFixGeometry(Geometry? geometry)
        {
            if (geometry == null || geometry.IsEmpty)
                return geometry;

            try
            {
                // Se a geometria já é válida, retorna ela mesma
                if (geometry.IsValid)
                    return geometry;

                // Tenta corrigir usando Buffer(0) - técnica comum para corrigir topologia
                var fixed1 = geometry.Buffer(0);
                if (fixed1.IsValid)
                    return fixed1;

                // Tenta usar Normalize + Buffer
                var normalized = (Geometry)geometry.Copy();
                normalized.Normalize();
                var fixed2 = normalized.Buffer(0);
                if (fixed2.IsValid)
                    return fixed2;

                // Última tentativa: simplificar ligeiramente
                var simplified = NetTopologySuite.Simplify.DouglasPeuckerSimplifier.Simplify(geometry, 0.0001);
                var fixed3 = simplified.Buffer(0);
                if (fixed3.IsValid)
                    return fixed3;

                // Se nada funcionou, retorna o resultado do buffer(0) mesmo que inválido
                return fixed1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao validar geometria: {ex.Message}");
                // Em caso de erro, tenta retornar um buffer mínimo
                try
                {
                    return geometry.Buffer(0);
                }
                catch
                {
                    return geometry;
                }
            }
        }

        /// <summary>
        /// Faz parse de uma geometria a partir de um JsonElement do GeoJSON
        /// </summary>
        private Geometry? ParseGeometryFromJson(JsonElement geometryElement)
        {
            try
            {
                if (!geometryElement.TryGetProperty("type", out var typeElement))
                {
                    return null;
                }

                var geometryType = typeElement.GetString();
                
                if (!geometryElement.TryGetProperty("coordinates", out var coordinatesElement))
                {
                    return null;
                }

                switch (geometryType)
                {
                    case "Polygon":
                        return ParsePolygonFromJson(coordinatesElement);
                    
                    case "MultiPolygon":
                        return ParseMultiPolygonFromJson(coordinatesElement);
                    
                    case "Point":
                        return ParsePointFromJson(coordinatesElement);
                    
                    default:
                        Console.WriteLine($"Tipo de geometria não suportado: {geometryType}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer parse da geometria: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Faz parse de um Point a partir de coordenadas JSON
        /// </summary>
        private Point? ParsePointFromJson(JsonElement coordinatesElement)
        {
            var coords = coordinatesElement.EnumerateArray().Select(c => c.GetDouble()).ToArray();
            if (coords.Length >= 2)
            {
                return _geometryFactory.CreatePoint(new Coordinate(coords[0], coords[1]));
            }
            return null;
        }

        /// <summary>
        /// Faz parse de um Polygon a partir de coordenadas JSON
        /// </summary>
        private Polygon? ParsePolygonFromJson(JsonElement coordinatesElement)
        {
            var rings = new List<LinearRing>();
            
            foreach (var ringElement in coordinatesElement.EnumerateArray())
            {
                var coordinates = new List<Coordinate>();
                foreach (var coordElement in ringElement.EnumerateArray())
                {
                    var coords = coordElement.EnumerateArray().Select(c => c.GetDouble()).ToArray();
                    if (coords.Length >= 2)
                    {
                        coordinates.Add(new Coordinate(coords[0], coords[1]));
                    }
                }
                
                if (coordinates.Count >= 4) // Polígono precisa de pelo menos 4 pontos (fechado)
                {
                    // Garantir que o anel está fechado
                    if (!coordinates.First().Equals2D(coordinates.Last()))
                    {
                        coordinates.Add(coordinates.First());
                    }
                    
                    rings.Add(_geometryFactory.CreateLinearRing(coordinates.ToArray()));
                }
            }

            if (rings.Count == 0)
            {
                return null;
            }

            // Primeiro anel é o externo, demais são buracos
            var shell = rings[0];
            var holes = rings.Skip(1).ToArray();
            
            return _geometryFactory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Faz parse de um MultiPolygon a partir de coordenadas JSON
        /// </summary>
        private MultiPolygon? ParseMultiPolygonFromJson(JsonElement coordinatesElement)
        {
            var polygons = new List<Polygon>();
            
            foreach (var polygonElement in coordinatesElement.EnumerateArray())
            {
                var polygon = ParsePolygonFromJson(polygonElement);
                if (polygon != null)
                {
                    polygons.Add(polygon);
                }
            }

            if (polygons.Count == 0)
            {
                return null;
            }

            return _geometryFactory.CreateMultiPolygon(polygons.ToArray());
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

                Console.WriteLine($"Iniciando geração de pontos. QtdPontosNaArea: {pontosPorHex}");

                // Obter a coleção de features original para acessar os IDs dos hexágonos
                NetTopologySuite.Features.FeatureCollection featureCollection;
                try
                {
                    // Parse manual do JsonElement para criar FeatureCollection
                    featureCollection = new NetTopologySuite.Features.FeatureCollection();
                    
                    if (!dados.GeoJsonAreas.TryGetProperty("features", out var featuresElement))
                    {
                        throw new Exception("GeoJSON não contém propriedade 'features'");
                    }

                    Console.WriteLine($"Parseando {featuresElement.GetArrayLength()} features...");

                    foreach (var featureElement in featuresElement.EnumerateArray())
                    {
                        // Extrair propriedades
                        var attributes = new NetTopologySuite.Features.AttributesTable();
                        if (featureElement.TryGetProperty("properties", out var propsElement))
                        {
                            if (propsElement.TryGetProperty("id", out var idElement))
                            {
                                attributes.Add("id", idElement.GetInt32());
                            }
                            if (propsElement.TryGetProperty("type", out var typeElement))
                            {
                                attributes.Add("type", typeElement.GetString());
                            }
                        }

                        // Extrair geometria
                        if (!featureElement.TryGetProperty("geometry", out var geometryElement))
                        {
                            continue;
                        }

                        var geometry = ParseGeometryFromJson(geometryElement);
                        if (geometry != null)
                        {
                            var feature = new NetTopologySuite.Features.Feature(geometry, attributes);
                            featureCollection.Add(feature);
                        }
                    }

                    Console.WriteLine($"GeoJSON parseado com sucesso. Total de features: {featureCollection.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao fazer parse do GeoJSON: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw new Exception($"Failed to correctly read json: {ex.Message}");
                }

                // 1) Pré-cálculo das áreas (em UTM) por hexágono e alocação proporcional de pontos
                var areas = new double[featureCollection.Count];
                int validCount = 0;
                double totalArea = 0.0;
                for (int fIdx = 0; fIdx < featureCollection.Count; fIdx++)
                {
                    var feature = featureCollection[fIdx];
                    double area = 0.0;

                    if (feature.Geometry is Polygon p)
                    {
                        var pUtm = TransformPolygon(p, transformToUtm);
                        area = Math.Max(0, pUtm.Area);
                    }
                    else if (feature.Geometry is MultiPolygon mp)
                    {
                        var polysUtm = mp.Geometries.Cast<Polygon>().Select(q => TransformPolygon(q, transformToUtm));
                        area = polysUtm.Sum(q => Math.Max(0, q.Area));
                    }

                    areas[fIdx] = area;
                    if (area > 0)
                    {
                        totalArea += area;
                        validCount++;
                    }
                }

                // Interpretação: QtdPontosNaArea = pontos médios por hex; total = média * número de hex válidos
                int totalPoints = Math.Max(0, pontosPorHex * Math.Max(1, validCount));
                double avgArea = validCount > 0 ? totalArea / validCount : 0.0;

                var allocationByIndex = new Dictionary<int, int>();
                for (int i = 0; i < featureCollection.Count; i++)
                {
                    int alloc;
                    if (avgArea <= 0 || areas[i] <= 0 || pontosPorHex <= 0)
                    {
                        alloc = 0;
                    }
                    else
                    {
                        // proporcional à área relativa vs área média, garantindo ao menos 1
                        alloc = Math.Max(1, (int)Math.Round((areas[i] / avgArea) * pontosPorHex));
                    }
                    allocationByIndex[i] = alloc;
                }

                // 2) Processar cada feature (hexágono) com alocação proporcional
                for (int fIdx = 0; fIdx < featureCollection.Count; fIdx++)
                {
                    var feature = featureCollection[fIdx];
                    int hexagonId = GetHexagonId(featureCollection, fIdx);

                    int pontosAlocados = allocationByIndex.GetValueOrDefault(fIdx, 0);
                    if (pontosAlocados <= 0)
                    {
                        perHexCounts[hexagonId] = 0;
                        Console.WriteLine($"Hexágono {hexagonId}: 0 pontos alocados (área muito pequena ou inválida)");
                        continue;
                    }

                    Console.WriteLine($"Processando hexágono {hexagonId}: alocados {pontosAlocados} pontos, área = {areas[fIdx]:F2} m²");

                    List<Coordinate> pontosGerados = new();
                    string metodoUsado = "triangulacao";

                    try
                    {
                        if (feature.Geometry is Polygon poly)
                        {
                            // Transformar para UTM
                            var polyUtm = TransformPolygon(poly, transformToUtm);
                            pontosGerados = GenerateExactPointsForPolygon(polyUtm, pontosAlocados, random, out metodoUsado);
                        }
                        else if (feature.Geometry is MultiPolygon multi)
                        {
                            // Transformar cada componente para UTM e distribuir por área
                            var polysUtm = multi.Geometries.Cast<Polygon>().Select(p => TransformPolygon(p, transformToUtm)).ToList();
                            pontosGerados = DistributePointsInMultiPolygon(polysUtm, pontosAlocados, random);
                            metodoUsado = "multipolygon_distribution";
                        }
                        else
                        {
                            // Geometria não suportada
                            Console.WriteLine($"Hexágono {hexagonId}: Tipo de geometria não suportado: {feature.Geometry?.GeometryType ?? "null"}");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar hexágono {hexagonId}: {ex.Message}");
                        pontosGerados = new List<Coordinate>();
                    }

                    Console.WriteLine($"Hexágono {hexagonId}: gerados {pontosGerados.Count} pontos usando método '{metodoUsado}'");

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

                Console.WriteLine($"Total de pontos gerados: {pointsWithHexagonId.Count}");
                Console.WriteLine($"Métodos utilizados: {string.Join(", ", metodosUsados)}");


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
            metodoUsado = "unknown"; // Inicialização padrão

            if (numPoints <= 0)
            {
                metodoUsado = "none";
                return new List<Coordinate>();
            }

            // Validar e corrigir o polígono antes de processar
            var validatedPolygon = ValidateAndFixGeometry(polygon) as Polygon;
            if (validatedPolygon == null || validatedPolygon.IsEmpty || validatedPolygon.Area <= 0)
            {
                metodoUsado = "invalid_polygon";
                return new List<Coordinate>();
            }

            List<Coordinate> points = new();

            try
            {
                // Tenta gerar pontos organizados (distribuição uniforme) usando candidatos via triangulação
                points = GenerateEvenlyDistributedPoints(validatedPolygon, numPoints, random);

                if (points.Count >= numPoints)
                {
                    // Lloyd relaxation (centroidal Voronoi) for more regular spacing
                    points = LloydRelaxation(validatedPolygon, points, iterations: 3);
                    metodoUsado = "even_farthest+lloyd3+triangulation_candidates";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em GenerateEvenlyDistributedPoints: {ex.Message}");
                points = new List<Coordinate>();
            }

            // Se falhou ou não gerou pontos suficientes, usar triangulação simples
            if (points.Count < numPoints)
            {
                try
                {
                    var triangulationPoints = GeneratePointsByTriangulation(validatedPolygon, numPoints, random);
                    if (triangulationPoints.Count > points.Count)
                    {
                        points = triangulationPoints;
                        metodoUsado = "triangulation_simple";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro em GeneratePointsByTriangulation: {ex.Message}");
                }
            }

            // Se ainda não tem pontos suficientes, usar rejection sampling
            if (points.Count < numPoints)
            {
                try
                {
                    var fallback = GeneratePointsByRejectionSampling(validatedPolygon, numPoints - points.Count, random);
                    points.AddRange(fallback);
                    string currentMethod = points.Count > 0 && metodoUsado != "unknown" ? metodoUsado + "+" : "";
                    metodoUsado = currentMethod + "rejection_sampling";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro em GeneratePointsByRejectionSampling: {ex.Message}");
                }
            }

            // Se ainda não tem nenhum ponto, usar fallback determinístico
            if (points.Count == 0)
            {
                try
                {
                    points = GenerateDeterministicFallbackPoints(validatedPolygon, numPoints, random);
                    metodoUsado = "deterministic_fallback";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro em GenerateDeterministicFallbackPoints: {ex.Message}");
                    // Último recurso: usar o centroide
                    var centroid = validatedPolygon.Centroid;
                    if (centroid != null && !centroid.IsEmpty)
                    {
                        points.Add(new Coordinate(centroid.X, centroid.Y));
                        metodoUsado = "centroid_only";
                    }
                    else
                    {
                        metodoUsado = "failed";
                    }
                }
            }

            // Garantir exatamente N pontos (ou o máximo que conseguimos)
            if (points.Count > numPoints)
            {
                points = points.Take(numPoints).ToList();
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

            if (polygons == null || polygons.Count == 0 || numPoints <= 0)
            {
                return allPoints;
            }

            // Validar e corrigir cada polígono
            var validPolygons = polygons
                .Select(p => ValidateAndFixGeometry(p) as Polygon)
                .Where(p => p != null && !p.IsEmpty && p.Area > 0)
                .Cast<Polygon>()
                .ToList();

            if (validPolygons.Count == 0)
            {
                return allPoints;
            }

            // Calcular áreas dos polígonos
            var polygonAreas = validPolygons.Select(p => p.Area).ToList();
            var totalArea = polygonAreas.Sum();

            if (totalArea <= 0)
            {
                return allPoints;
            }

            // Distribuir pontos proporcionalmente
            var pointsDistributed = 0;
            for (int i = 0; i < validPolygons.Count && pointsDistributed < numPoints; i++)
            {
                var polygon = validPolygons[i];
                var area = polygonAreas[i];

                // Calcular quantos pontos este polígono deve receber
                int pointsForPolygon;
                if (i == validPolygons.Count - 1) // Último polígono recebe os pontos restantes
                {
                    pointsForPolygon = numPoints - pointsDistributed;
                }
                else
                {
                    pointsForPolygon = Math.Max(1, (int)Math.Round((area / totalArea) * numPoints));
                }

                if (pointsForPolygon <= 0)
                    continue;

                // Gerar pontos no polígono usando método robusto
                string metodoUsado;
                var polygonPoints = GenerateExactPointsForPolygon(polygon, pointsForPolygon, random, out metodoUsado);

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
                Coordinate? point = null;
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

        /// <summary>
        /// Geração de pontos organizados: seleciona N pontos maximizando a distância mínima entre eles e das bordas
        /// Usa candidatos gerados por triangulação (amostragem uniforme por área dos triângulos)
        /// </summary>
        private List<Coordinate> GenerateEvenlyDistributedPoints(Polygon polygon, int numPoints, Random random)
        {
            // 1) Gerar muitos candidatos dentro do polígono (triangulação + baricêntricas)
            var triangles = TriangulatePolygon(polygon);
            if (triangles.Count == 0)
            {
                return new List<Coordinate>();
            }

            // número de candidatos: p.ex. 20x N (limitado)
            int candidateCount = Math.Clamp(numPoints * 20, numPoints, numPoints * 100);
            var candidates = DistributePointsInTriangles(triangles, candidateCount, random);

            if (candidates.Count == 0)
                return candidates;

            // 2) Farthest-Point Sampling (greedy): seleciona pontos maximizando espaçamento
            var prepared = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(polygon);

            // dist mínima ao contorno para penalizar bordas
            double EdgePenalty(Coordinate c)
            {
                // distância euclidiana até o polígono (0 se dentro), usamos distância até o limite
                var pnt = new Point(c.X, c.Y) { SRID = polygon.SRID };
                // usa Distance para aproximar distância ao contorno
                return polygon.Boundary.Distance(pnt);
            }

            // Inicialização: pega o candidato com maior distância à borda
            var distToEdge = candidates.Select(EdgePenalty).ToArray();
            int firstIdx = 0;
            double best = double.NegativeInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (distToEdge[i] > best)
                {
                    best = distToEdge[i];
                    firstIdx = i;
                }
            }

            var selected = new List<Coordinate> { candidates[firstIdx] };

            if (numPoints == 1)
                return selected;

            // Manter, para cada candidato, a distância mínima para o conjunto selecionado
            var minDist = new double[candidates.Count];
            for (int i = 0; i < candidates.Count; i++)
            {
                minDist[i] = Distance(candidates[i], selected[0]);
            }

            // Greedy farthest: escolher sempre o candidato que maximiza minDist + pesoEdge
            for (int k = 1; k < numPoints; k++)
            {
                int bestIdx = -1;
                double bestScore = double.NegativeInfinity;

                for (int i = 0; i < candidates.Count; i++)
                {
                    // penalizar pontos muito próximos da borda
                    double score = minDist[i] + 0.25 * distToEdge[i];
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIdx = i;
                    }
                }

                if (bestIdx == -1)
                    break;

                var chosen = candidates[bestIdx];
                if (!prepared.Contains(new Point(chosen) { SRID = polygon.SRID }))
                {
                    continue;
                }

                selected.Add(chosen);

                // atualizar minDist
                for (int i = 0; i < candidates.Count; i++)
                {
                    double d = Distance(candidates[i], chosen);
                    if (d < minDist[i]) minDist[i] = d;
                }
            }

            return selected;
        }

        private List<Coordinate> LloydRelaxation(Polygon polygon, List<Coordinate> sites, int iterations = 2)
        {
            if (sites == null || sites.Count == 0 || iterations <= 0)
                return sites ?? new List<Coordinate>();

            // Ensure all sites are inside polygon; if not, filter
            var prepared = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(polygon);
            var current = sites.Where(c => prepared.Contains(new Point(c) { SRID = polygon.SRID })).ToList();
            if (current.Count == 0)
                return sites; // fallback to original if all filtered out

            for (int it = 0; it < iterations; it++)
            {
                var builder = new VoronoiDiagramBuilder();
                builder.SetSites(current);
                // Some NTS versions don't expose SetClipEnvelope; we'll intersect cells with polygon instead
                var diagram = builder.GetDiagram(_geometryFactory);

                var next = new List<Coordinate>(current.Count);

                for (int i = 0; i < diagram.NumGeometries; i++)
                {
                    var cell = diagram.GetGeometryN(i);
                    // Clip the cell to the target polygon to keep centroids inside
                    var clipped = cell.Intersection(polygon);

                    if (clipped == null || clipped.IsEmpty || clipped.Area <= 0)
                    {
                        // Keep original site if clipping failed
                        if (i < current.Count)
                            next.Add(current[i]);
                        continue;
                    }

                    // Use centroid; if not inside (degenerate), fallback to interior point
                    var centroid = clipped.Centroid;
                    Coordinate newCoord;
                    if (centroid == null || centroid.IsEmpty || !prepared.Contains(centroid))
                    {
                        var interior = clipped.PointOnSurface;
                        newCoord = interior?.Coordinate ?? (i < current.Count ? current[i] : current.Last());
                    }
                    else
                    {
                        newCoord = centroid.Coordinate;
                    }

                    next.Add(newCoord);
                }

                // Maintain same count as original; if diagram produced more cells than sites, trim; if fewer, pad with existing
                if (next.Count > sites.Count)
                    next = next.Take(sites.Count).ToList();
                else if (next.Count < sites.Count)
                {
                    // pad deterministically with nearest existing
                    next.AddRange(current.Skip(next.Count).Take(sites.Count - next.Count));
                }

                current = next;
            }

            // Final pass: ensure all points are strictly inside polygon
            var final = new List<Coordinate>(current.Count);
            foreach (var c in current)
            {
                var p = new Point(c) { SRID = polygon.SRID };
                if (prepared.Contains(p))
                {
                    final.Add(c);
                }
                else
                {
                    // snap to interior using point on surface
                    final.Add(polygon.PointOnSurface.Coordinate);
                }
            }

            return final;
        }

        private static double Distance(Coordinate a, Coordinate b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion


    }
}
