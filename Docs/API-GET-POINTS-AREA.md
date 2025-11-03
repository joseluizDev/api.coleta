# API - Endpoint `get-points-area`

## 📋 Sumário

- [Visão Geral](#visão-geral)
- [Endpoint](#endpoint)
- [Estrutura de Request](#estrutura-de-request)
- [Estrutura de Response](#estrutura-de-response)
- [Como Funciona](#como-funciona)
- [Algoritmos Utilizados](#algoritmos-utilizados)
- [Exemplos de Uso](#exemplos-de-uso)
- [Tratamento de Erros](#tratamento-de-erros)

---

## 🎯 Visão Geral

O endpoint `get-points-area` é responsável por **gerar pontos de coleta distribuídos uniformemente dentro de áreas geográficas** (hexágonos ou polígonos). Este endpoint é essencial para:

- Planejamento de coletas de solo em propriedades rurais
- Distribuição uniforme de pontos de amostragem
- Geração determinística e reproduzível de pontos (via seed)
- Otimização de rotas de coleta

### Características Principais:

- ✅ Distribuição uniforme e organizada dos pontos
- ✅ Suporte a múltiplas áreas (MultiPolygon/FeatureCollection)
- ✅ Alocação proporcional baseada em área
- ✅ Geração determinística (via seed)
- ✅ Múltiplos algoritmos com fallback automático

---

## 🌐 Endpoint

### POST `/api/utils/get-points-area`

**Descrição:** Gera pontos de coleta distribuídos uniformemente dentro de áreas geográficas fornecidas.

**Content-Type:** `application/json`

---

## 📥 Estrutura de Request

### Request DTO

```csharp
public class PontosDentroDaAreaRequest
{
    public JsonElement GeoJsonAreas { get; set; }  // FeatureCollection com hexágonos/polígonos
    public int QtdPontosNaArea { get; set; }        // Quantidade média de pontos por área
    public int? Seed { get; set; }                  // Seed opcional para determinismo
}
```

### Campos

| Campo             | Tipo          | Obrigatório | Descrição                                                                  |
| ----------------- | ------------- | ----------- | -------------------------------------------------------------------------- |
| `GeoJsonAreas`    | `JsonElement` | ✅ Sim      | FeatureCollection GeoJSON contendo as áreas (hexágonos/polígonos)          |
| `QtdPontosNaArea` | `int`         | ✅ Sim      | Quantidade **média** de pontos por hexágono/área                           |
| `Seed`            | `int?`        | ❌ Não      | Seed para geração determinística (usar o mesmo seed gera os mesmos pontos) |

### Exemplo de Request Body

```json
{
  "geoJsonAreas": {
    "type": "FeatureCollection",
    "features": [
      {
        "type": "Feature",
        "properties": {
          "id": 1,
          "type": "hexagon"
        },
        "geometry": {
          "type": "Polygon",
          "coordinates": [
            [
              [-47.91234, -21.12345],
              [-47.912, -21.124],
              [-47.9115, -21.1238],
              [-47.9118, -21.1232],
              [-47.91234, -21.12345]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 2,
          "type": "hexagon"
        },
        "geometry": {
          "type": "Polygon",
          "coordinates": [
            [
              [-47.913, -21.125],
              [-47.9127, -21.1255],
              [-47.9122, -21.1253],
              [-47.9125, -21.1247],
              [-47.913, -21.125]
            ]
          ]
        }
      }
    ]
  },
  "qtdPontosNaArea": 5,
  "seed": 12345
}
```

---

## 📤 Estrutura de Response

### Response DTO

```csharp
public class PontosDentroDaAreaResponse
{
    public JsonElement Points { get; set; }              // Array de Features GeoJSON (pontos)
    public PontosDentroDaAreaMeta Meta { get; set; }     // Metadados da geração
}

public class PontosDentroDaAreaMeta
{
    public Dictionary<int, int> PerHexCounts { get; set; }  // ID hexágono -> qtd pontos
    public int SeedUsado { get; set; }                       // Seed utilizado
    public string Metodo { get; set; }                       // Método(s) de geração usado(s)
}
```

### Campos da Response

| Campo               | Tipo                   | Descrição                                                                     |
| ------------------- | ---------------------- | ----------------------------------------------------------------------------- |
| `Points`            | `JsonElement`          | Array de Features GeoJSON representando os pontos gerados                     |
| `Meta.PerHexCounts` | `Dictionary<int, int>` | Mapa com ID do hexágono e quantidade de pontos gerados nele                   |
| `Meta.SeedUsado`    | `int`                  | Seed utilizado na geração (útil para reproduzir resultados)                   |
| `Meta.Metodo`       | `string`               | Método(s) de geração utilizado(s) (ex: "triangulation", "rejection_sampling") |

### Estrutura dos Pontos (GeoJSON Features)

Cada ponto no array `Points` tem a seguinte estrutura:

```json
{
  "type": "Feature",
  "properties": {
    "type": "point",
    "id": 1,
    "hexagonId": 1,
    "coletado": false
  },
  "geometry": {
    "type": "Point",
    "coordinates": [-47.91215, -21.1236]
  }
}
```

### Exemplo de Response Completa

```json
{
  "points": [
    {
      "type": "Feature",
      "properties": {
        "type": "point",
        "id": 1,
        "hexagonId": 1,
        "coletado": false
      },
      "geometry": {
        "type": "Point",
        "coordinates": [-47.91215, -21.1236]
      }
    },
    {
      "type": "Feature",
      "properties": {
        "type": "point",
        "id": 2,
        "hexagonId": 1,
        "coletado": false
      },
      "geometry": {
        "type": "Point",
        "coordinates": [-47.912, -21.1237]
      }
    }
  ],
  "meta": {
    "perHexCounts": {
      "1": 5,
      "2": 5
    },
    "seedUsado": 12345,
    "metodo": "even_farthest+lloyd3+triangulation_candidates"
  }
}
```

**Nota:** O controller mantém compatibilidade retornando apenas o array `Points` diretamente (sem o wrapper), mas o serviço retorna a resposta completa com metadados.

---

## ⚙️ Como Funciona

### 1️⃣ **Recebimento e Validação**

```csharp
[HttpPost("get-points-area")]
public IActionResult GetPointsInsideArea([FromBody] PontosDentroDaAreaRequest request)
{
    var result = _utilsService.GetPointsInsideArea(request);

    if (result.Points.GetArrayLength() == 0)
    {
        return BadRequest(new { error = "Nenhum ponto foi gerado dentro da área especificada." });
    }

    return Ok(result.Points); // Retorna apenas o array de pontos (compatibilidade)
}
```

### 2️⃣ **Processamento no Service**

O `UtilsService.GetPointsInsideArea` executa os seguintes passos:

#### **Passo 1: Configuração e Parsing**

```csharp
// Configura seed para determinismo
int seed = dados.Seed ?? Environment.TickCount;
var random = new Random(seed);

// Parse do GeoJSON para FeatureCollection
var featureCollection = ParseGeoJsonToFeatureCollection(dados.GeoJsonAreas);
```

#### **Passo 2: Cálculo de Alocação Proporcional**

```csharp
// Calcula área de cada hexágono em UTM
for (int fIdx = 0; fIdx < featureCollection.Count; fIdx++)
{
    var polygon = featureCollection[fIdx].Geometry as Polygon;
    var polygonUtm = TransformPolygon(polygon, transformToUtm);
    areas[fIdx] = polygonUtm.Area;
    totalArea += areas[fIdx];
}

// Aloca pontos proporcionalmente
double avgArea = totalArea / validCount;
foreach (var area in areas)
{
    int allocation = Math.Max(1, (int)Math.Round((area / avgArea) * QtdPontosNaArea));
    allocationByIndex[index] = allocation;
}
```

**Interpretação de `QtdPontosNaArea`:**

- É a quantidade **média** de pontos por hexágono
- Total de pontos = `QtdPontosNaArea × número de hexágonos válidos`
- Cada hexágono recebe proporcionalmente mais ou menos pontos baseado em sua área

#### **Passo 3: Geração de Pontos por Hexágono**

```csharp
foreach (var feature in featureCollection)
{
    int pontosAlocados = allocationByIndex[index];

    // Transforma para UTM para cálculos precisos
    var polygonUtm = TransformPolygon(polygon, transformToUtm);

    // Gera pontos usando algoritmo robusto
    var pontos = GenerateExactPointsForPolygon(polygonUtm, pontosAlocados, random, out metodo);

    // Armazena pontos com ID do hexágono
    pointsWithHexagonId.AddRange(pontos.Select(p => (p, hexagonId)));
}
```

#### **Passo 4: Conversão para GeoJSON**

```csharp
// Transforma pontos de volta para WGS84 (lat/lon)
var transform = GetUtmToWgs84(referenceGeometry);

foreach (var (point, hexagonId) in pointsWithHexagonId)
{
    var wgs84Point = transform.Transform([point.X, point.Y]);

    features.Add(new {
        type = "Feature",
        properties = new {
            type = "point",
            id = pointId++,
            hexagonId = hexagonId,
            coletado = false
        },
        geometry = new {
            type = "Point",
            coordinates = [wgs84Point[0], wgs84Point[1]]
        }
    });
}
```

---

## 🧮 Algoritmos Utilizados

O sistema utiliza múltiplos algoritmos com **fallback automático** para garantir robustez:

### 1️⃣ **Distribuição Uniforme com Lloyd Relaxation** (Primário)

**Método:** `even_farthest+lloyd3+triangulation_candidates`

```csharp
private List<Coordinate> GenerateEvenlyDistributedPoints(Polygon polygon, int numPoints, Random random)
{
    // 1. Triangula o polígono
    var triangles = TriangulatePolygon(polygon);

    // 2. Gera muitos candidatos (20x N) distribuídos uniformemente por área
    int candidateCount = Math.Clamp(numPoints * 20, numPoints, numPoints * 100);
    var candidates = DistributePointsInTriangles(triangles, candidateCount, random);

    // 3. Farthest-Point Sampling: seleciona N pontos maximizando espaçamento
    var selected = FarthestPointSampling(candidates, numPoints, polygon);

    // 4. Lloyd Relaxation (3 iterações): refina posições usando Voronoi
    selected = LloydRelaxation(polygon, selected, iterations: 3);

    return selected;
}
```

**Vantagens:**

- ✅ Distribuição mais uniforme possível
- ✅ Pontos bem espaçados
- ✅ Evita clustering
- ✅ Considera distância às bordas

**Técnicas:**

- **Triangulação (LibTessDotNet):** Divide o polígono em triângulos
- **Coordenadas Baricêntricas:** Gera pontos uniformemente dentro de triângulos
- **Farthest-Point Sampling:** Algoritmo guloso que maximiza distância mínima
- **Lloyd Relaxation:** Usa Voronoi para "relaxar" pontos (centroidal Voronoi tessellation)

### 2️⃣ **Triangulação Simples** (Fallback 1)

**Método:** `triangulation_simple`

```csharp
private List<Coordinate> GeneratePointsByTriangulation(Polygon polygon, int numPoints, Random random)
{
    var triangles = TriangulatePolygon(polygon);
    return DistributePointsInTriangles(triangles, numPoints, random);
}
```

**Quando é usado:**

- Se o algoritmo primário falhar
- Para polígonos muito complexos onde Lloyd falha

### 3️⃣ **Rejection Sampling** (Fallback 2)

**Método:** `rejection_sampling`

```csharp
private List<Coordinate> GeneratePointsByRejectionSampling(Polygon polygon, int numPoints, Random random)
{
    var bounds = polygon.EnvelopeInternal;
    var prepared = PreparedGeometryFactory.Prepare(polygon);

    while (pointsGenerated < numPoints && attempts < maxAttempts)
    {
        // Gera ponto aleatório no bounding box
        double x = bounds.MinX + random.NextDouble() * (bounds.MaxX - bounds.MinX);
        double y = bounds.MinY + random.NextDouble() * (bounds.MaxY - bounds.MinY);

        // Aceita se dentro do polígono
        if (prepared.Contains(new Point(x, y)))
        {
            points.Add(new Coordinate(x, y));
            pointsGenerated++;
        }
        attempts++;
    }

    return points;
}
```

**Quando é usado:**

- Se triangulação falhar
- Como complemento para atingir quantidade exata de pontos

### 4️⃣ **Fallback Determinístico** (Último Recurso)

**Método:** `deterministic_fallback` ou `centroid_only`

```csharp
private List<Coordinate> GenerateDeterministicFallbackPoints(Polygon polygon, int numPoints, Random random)
{
    var centroid = polygon.Centroid;
    var maxJitter = Math.Min(bounds.Width, bounds.Height) * 0.1;

    for (int i = 0; i < numPoints; i++)
    {
        // Gera pontos próximos ao centroide com jitter aleatório
        double jitterX = (random.NextDouble() - 0.5) * 2 * maxJitter;
        double jitterY = (random.NextDouble() - 0.5) * 2 * maxJitter;

        points.Add(new Coordinate(centroid.X + jitterX, centroid.Y + jitterY));
    }

    return points;
}
```

**Quando é usado:**

- Quando todos os outros métodos falharam
- Para geometrias muito degeneradas

### 📊 Diagrama de Fluxo dos Algoritmos

```
┌─────────────────────────────────────────┐
│ GenerateExactPointsForPolygon           │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│ 1. GenerateEvenlyDistributedPoints      │
│    - Triangulação                       │
│    - Candidatos (20x N)                 │
│    - Farthest-Point Sampling            │
│    - Lloyd Relaxation (3x)              │
└─────────────────┬───────────────────────┘
                  │
          ┌───────┴───────┐
          │ Sucesso?      │
          └───────┬───────┘
                  │ Não
                  ▼
┌─────────────────────────────────────────┐
│ 2. GeneratePointsByTriangulation        │
│    - Triangulação simples               │
│    - Distribuição por área              │
└─────────────────┬───────────────────────┘
                  │
          ┌───────┴───────┐
          │ Sucesso?      │
          └───────┬───────┘
                  │ Não
                  ▼
┌─────────────────────────────────────────┐
│ 3. GeneratePointsByRejectionSampling    │
│    - Gera aleatório no bbox             │
│    - Testa se está dentro               │
└─────────────────┬───────────────────────┘
                  │
          ┌───────┴───────┐
          │ Sucesso?      │
          └───────┬───────┘
                  │ Não
                  ▼
┌─────────────────────────────────────────┐
│ 4. GenerateDeterministicFallbackPoints  │
│    - Centroide + jitter                 │
└─────────────────────────────────────────┘
```

---

## 🔧 Transformações de Coordenadas

### Por que usar UTM?

O sistema trabalha em **duas projeções**:

1. **WGS84 (entrada/saída):** Latitude/Longitude (graus)
2. **UTM (processamento):** Coordenadas planas (metros)

```csharp
// Entrada: WGS84 -> UTM
var transformToUtm = GetWgs84ToUtm(geometry);
var polygonUtm = TransformPolygon(polygon, transformToUtm);

// ... processamento em UTM (cálculos de área, distância) ...

// Saída: UTM -> WGS84
var transformToWgs84 = GetUtmToWgs84(geometry);
var wgs84Coords = transform.Transform(new[] { point.X, point.Y });
```

**Vantagens do UTM:**

- ✅ Cálculos de área precisos em m²
- ✅ Distâncias euclidianas corretas
- ✅ Evita distorções de latitude/longitude
- ✅ Zona UTM calculada automaticamente pelo centroide

### Cálculo Automático de Zona UTM

```csharp
private MathTransform GetWgs84ToUtm(Geometry geometry)
{
    var centroid = geometry.Centroid;

    // Zona UTM baseada na longitude
    int zone = (int)Math.Floor((centroid.X + 180) / 6) + 1;

    // Hemisfério baseado na latitude
    bool isSouth = centroid.Y < 0;

    return _ctFactory.CreateFromCoordinateSystems(
        GeographicCoordinateSystem.WGS84,
        ProjectedCoordinateSystem.WGS84_UTM(zone, isSouth)
    ).MathTransform;
}
```

---

## 📚 Bibliotecas Utilizadas

| Biblioteca           | Versão | Propósito                                     |
| -------------------- | ------ | --------------------------------------------- |
| **NetTopologySuite** | -      | Operações geométricas, validação de polígonos |
| **LibTessDotNet**    | -      | Triangulação robusta de polígonos             |
| **ProjNet**          | -      | Transformações de coordenadas (WGS84 ↔ UTM)   |

---

## 💡 Exemplos de Uso

### Exemplo 1: Request Básico

```bash
curl -X POST http://localhost:5000/api/utils/get-points-area \
  -H "Content-Type: application/json" \
  -d '{
    "geoJsonAreas": {
      "type": "FeatureCollection",
      "features": [/* hexágonos */]
    },
    "qtdPontosNaArea": 5
  }'
```

### Exemplo 2: Com Seed para Reprodutibilidade

```bash
curl -X POST http://localhost:5000/api/utils/get-points-area \
  -H "Content-Type: application/json" \
  -d '{
    "geoJsonAreas": {/* ... */},
    "qtdPontosNaArea": 10,
    "seed": 42
  }'
```

**Importante:** Usar o mesmo `seed` sempre gerará **exatamente os mesmos pontos**.

### Exemplo 3: C# Client

```csharp
var client = new HttpClient();
var request = new PontosDentroDaAreaRequest
{
    GeoJsonAreas = hexagonsGeoJson,
    QtdPontosNaArea = 5,
    Seed = 12345
};

var json = JsonSerializer.Serialize(request);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync(
    "http://localhost:5000/api/utils/get-points-area",
    content
);

var result = await response.Content.ReadAsStringAsync();
var points = JsonSerializer.Deserialize<JsonElement>(result);
```

---

## ⚠️ Tratamento de Erros

### Erros Comuns e Soluções

| Erro                                                    | Causa                                      | Solução                        |
| ------------------------------------------------------- | ------------------------------------------ | ------------------------------ |
| `"Nenhum ponto foi gerado dentro da área especificada"` | Geometria inválida ou áreas muito pequenas | Verificar GeoJSON de entrada   |
| `"Failed to correctly read json"`                       | GeoJSON malformado                         | Validar estrutura do GeoJSON   |
| `"Polígono inválido ou vazio"`                          | Geometria com problemas topológicos        | Usar `Buffer(0)` para corrigir |
| Poucos pontos gerados                                   | Área muito pequena ou complexa             | Aumentar `QtdPontosNaArea`     |

### Validação de Geometrias

O sistema **valida e corrige automaticamente** geometrias problemáticas:

```csharp
private Geometry ValidateAndFixGeometry(Geometry geometry)
{
    if (geometry.IsValid)
        return geometry;

    // Técnica 1: Buffer(0)
    var fixed = geometry.Buffer(0);
    if (fixed.IsValid)
        return fixed;

    // Técnica 2: Normalize + Buffer
    geometry.Normalize();
    fixed = geometry.Buffer(0);
    if (fixed.IsValid)
        return fixed;

    // Técnica 3: Simplificação
    var simplified = DouglasPeuckerSimplifier.Simplify(geometry, 0.0001);
    return simplified.Buffer(0);
}
```

### Logs e Debugging

O sistema gera logs detalhados:

```
Iniciando geração de pontos. QtdPontosNaArea: 5
Parseando 10 features...
GeoJSON parseado com sucesso. Total de features: 10
Processando hexágono 1: alocados 5 pontos, área = 12543.67 m²
Hexágono 1: gerados 5 pontos usando método 'even_farthest+lloyd3+triangulation_candidates'
...
Total de pontos gerados: 50
Métodos utilizados: even_farthest+lloyd3+triangulation_candidates
```

---

## 🎓 Conceitos Técnicos

### Coordenadas Baricêntricas

Técnica para gerar pontos uniformemente dentro de triângulos:

```csharp
private Coordinate GenerateRandomPointInTriangle(Vec3 p1, Vec3 p2, Vec3 p3, Random random)
{
    double u = random.NextDouble();
    double v = random.NextDouble();

    if (u + v > 1)
    {
        u = 1 - u;
        v = 1 - v;
    }

    double w = 1 - u - v;

    double x = u * p1.X + v * p2.X + w * p3.X;
    double y = u * p1.Y + v * p2.Y + w * p3.Y;

    return new Coordinate(x, y);
}
```

### Farthest-Point Sampling

Algoritmo guloso para maximizar espaçamento:

```
1. Selecionar ponto inicial (mais distante da borda)
2. Para cada iteração k = 2 até N:
   a. Para cada candidato, calcular distância ao ponto mais próximo já selecionado
   b. Selecionar candidato com MAIOR distância mínima
   c. Atualizar distâncias
3. Retornar N pontos selecionados
```

### Lloyd Relaxation (Centroidal Voronoi)

Processo iterativo que "relaxa" pontos para posições mais uniformes:

```
1. Criar diagrama de Voronoi com os pontos
2. Para cada célula de Voronoi:
   a. Calcular o centroide da célula
   b. Mover o ponto para o centroide
3. Repetir 2-3 iterações
```

---

## 📊 Performance

### Complexidade

| Operação                | Complexidade                         |
| ----------------------- | ------------------------------------ |
| Triangulação            | O(n log n)                           |
| Farthest-Point Sampling | O(k × m) onde k=pontos, m=candidatos |
| Lloyd Relaxation        | O(k × i) onde i=iterações            |
| Rejection Sampling      | O(n / density)                       |

### Otimizações

1. **Prepared Geometry:** Cache de índices espaciais para testes de contenção
2. **Alocação Proporcional:** Evita processar áreas muito pequenas
3. **Transformação UTM:** Apenas uma vez por conjunto de áreas
4. **Fallback Inteligente:** Métodos mais rápidos são tentados primeiro

---

## 🔗 Relacionados

- [GERACAO-GRID-HEXAGONAL.md](./GERACAO-GRID-HEXAGONAL.md) - Geração de hexágonos
- [OTIMIZACOES-MOBILE-COLETA.md](./OTIMIZACOES-MOBILE-COLETA.md) - Otimizações mobile

---

## 📅 Histórico de Versões

| Versão | Data    | Alterações                                            |
| ------ | ------- | ----------------------------------------------------- |
| 1.0    | 2025-01 | Versão inicial com rejection sampling                 |
| 2.0    | 2025-04 | Adicionado triangulação e alocação proporcional       |
| 3.0    | 2025-11 | Adicionado Lloyd relaxation e farthest-point sampling |

---

**Autor:** Equipe de Desenvolvimento  
**Última Atualização:** 03/11/2025
