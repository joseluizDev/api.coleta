# Documentação: Geração de Grid Hexagonal

## Visão Geral

Este documento descreve detalhadamente como funciona o algoritmo de geração de grid hexagonal implementado no sistema. O grid hexagonal é usado para dividir áreas geográficas (polígonos) em células hexagonais de tamanho específico, permitindo análise e coleta de dados espaciais.

## Índice

1. [Conceitos Básicos](#conceitos-básicos)
2. [Fluxo Principal](#fluxo-principal)
3. [Cálculo das Dimensões do Hexágono](#cálculo-das-dimensões-do-hexágono)
4. [Geração da Grade](#geração-da-grade)
5. [Transformações de Coordenadas](#transformações-de-coordenadas)
6. [Validação e Correção de Geometrias](#validação-e-correção-de-geometrias)
7. [Formato de Saída (GeoJSON)](#formato-de-saída-geojson)
8. [Exemplo de Uso](#exemplo-de-uso)

---

## Conceitos Básicos

### O que é um Grid Hexagonal?

Um grid hexagonal é uma malha de células hexagonais (seis lados) que cobrem uma área. Hexágonos são preferidos em muitas aplicações geoespaciais porque:

- Cada célula tem a mesma distância para seus vizinhos
- Minimizam a distorção em relação a círculos
- Proporcionam melhor cobertura de área do que grids quadrados

### Por que usar UTM?

O sistema UTM (Universal Transverse Mercator) é usado porque:

- Preserva distâncias e áreas em metros (melhor para cálculos)
- Minimiza distorções em áreas locais
- Facilita cálculos geométricos precisos

---

## Fluxo Principal

O processo de geração de hexágonos segue este fluxo:

```
1. Receber GeoJSON (polígono em WGS84) + Tamanho desejado (hectares)
   ↓
2. Parse do GeoJSON → Polígono (Geometry)
   ↓
3. Transformação WGS84 → UTM (zona 23S)
   ↓
4. Geração do Grid Hexagonal
   ↓
5. Interseção com o Polígono Original
   ↓
6. Transformação UTM → WGS84
   ↓
7. Conversão para GeoJSON (FeatureCollection)
```

### Código do Método Principal

```csharp
public JsonElement GenerateHexagons(JsonElement polygonGeoJson, double hectares)
{
    try
    {
        // 1. Parse do polígono do GeoJSON
        var inputPolygon = ParsePolygon(polygonGeoJson);

        // 2. Transformar de WGS84 para UTM
        var transformedPolygon = TransformPolygon(inputPolygon, GetWgs84ToUtm());

        // 3. Gerar grid hexagonal
        var hexagons = GenerateHexagonalGrid(transformedPolygon, hectares);

        // 4. Converter para GeoJSON
        var geoJson = ConvertHexagonsToGeoJson(hexagons);

        return geoJson;
    }
    catch (Exception ex)
    {
        throw new Exception("Erro ao gerar hexágonos: " + ex.Message);
    }
}
```

---

## Cálculo das Dimensões do Hexágono

### Fórmulas Matemáticas

Para criar hexágonos com uma área específica (em hectares), usamos as seguintes fórmulas:

#### 1. Conversão de Hectares para Metros Quadrados

```csharp
double areaM2 = hectares * 10000; // 1 hectare = 10.000 m²
```

#### 2. Cálculo do Raio (apótema ao vértice)

A área de um hexágono regular é:

```
A = (3 * √3 / 2) * r²
```

Resolvendo para `r`:

```csharp
double r = Math.Sqrt((2 * areaM2) / (3 * Math.Sqrt(3)));
```

#### 3. Dimensões do Hexágono

```csharp
double hexWidth = Math.Sqrt(3) * r;  // Largura (ponta a ponta horizontal)
double hexHeight = 2 * r;             // Altura (ponta a ponta vertical)
double vertDist = hexHeight * 0.75;   // Distância vertical entre centros
```

### Visualização das Dimensões

```
       ____
      /    \
     /      \    ← hexHeight = 2r
    |        |
     \      /    ← vertDist = 1.5r (75% da altura)
      \____/

    ←─────→
     hexWidth = √3 * r
```

---

## Geração da Grade

### Algoritmo de Posicionamento

O algoritmo cria uma grade regular com offset nas linhas ímpares:

```csharp
private List<Geometry> GenerateHexagonalGrid(Polygon projectedPolygon, double hectares)
{
    // Cálculo das dimensões
    double areaM2 = hectares * 10000;
    double r = Math.Sqrt((2 * areaM2) / (3 * Math.Sqrt(3)));
    double hexWidth = Math.Sqrt(3) * r;
    double hexHeight = 2 * r;
    double vertDist = hexHeight * 0.75;

    var bounds = projectedPolygon.EnvelopeInternal;
    var hexagons = new List<Geometry>();

    // Validar e corrigir o polígono
    var validatedPolygon = ValidateAndFixGeometry(projectedPolygon);
    var preparedPolygon = PreparedGeometryFactory.Prepare(validatedPolygon);

    // Iterar em linhas e colunas
    for (int row = 0; row < ((bounds.MaxY - bounds.MinY) / vertDist) + 1; row++)
    {
        for (int col = 0; col < ((bounds.MaxX - bounds.MinX) / hexWidth) + 1; col++)
        {
            // Offset para linhas ímpares (padrão honeycomb)
            double offset = (row % 2 == 0) ? 0 : hexWidth / 2;

            // Calcular centro do hexágono
            double centerX = bounds.MinX + col * hexWidth + offset;
            double centerY = bounds.MinY + row * vertDist;

            // Criar hexágono
            Polygon hexagon = CreateHexagon(new Coordinate(centerX, centerY), r);

            // Verificar interseção com o polígono original
            if (preparedPolygon.Intersects(hexagon))
            {
                var validatedHexagon = ValidateAndFixGeometry(hexagon);
                var intersection = validatedPolygon.Intersection(validatedHexagon);

                if (intersection != null && !intersection.IsEmpty && intersection.Area > 0)
                {
                    hexagons.Add(intersection);
                }
            }
        }
    }

    return hexagons;
}
```

### Padrão Honeycomb (Favo de Mel)

As linhas ímpares são deslocadas horizontalmente:

```
Linha 0:  ⬡  ⬡  ⬡  ⬡  ⬡
Linha 1:    ⬡  ⬡  ⬡  ⬡    ← offset = hexWidth / 2
Linha 2:  ⬡  ⬡  ⬡  ⬡  ⬡
Linha 3:    ⬡  ⬡  ⬡  ⬡    ← offset = hexWidth / 2
```

---

## Criação de Hexágonos Individuais

### Método CreateHexagon

Cria um hexágono regular com 6 vértices ao redor de um centro:

```csharp
private Polygon CreateHexagon(Coordinate center, double r)
{
    // Gera 6 vértices ao redor do centro
    // Começa em -30° e incrementa 60° por vértice
    var vertices = Enumerable.Range(0, 6)
        .Select(i => new Coordinate(
            center.X + r * Math.Cos(Math.PI / 180 * (60 * i - 30)),
            center.Y + r * Math.Sin(Math.PI / 180 * (60 * i - 30))
        ))
        .ToList();

    // Fechar o polígono (primeiro ponto = último ponto)
    vertices.Add(vertices.First());

    return _geometryFactory.CreatePolygon(vertices.ToArray());
}
```

### Orientação dos Vértices

Os vértices são gerados no sentido anti-horário, começando em -30°:

```
        V0 (30°)
       /      \
   V5 /        \ V1 (90°)
     |          |
   V4 \        / V2 (150°)
       \______/
        V3 (210°)
```

Ângulos: -30°, 30°, 90°, 150°, 210°, 270°

---

## Transformações de Coordenadas

### WGS84 para UTM

```csharp
private MathTransform GetWgs84ToUtm()
{
    return _ctFactory.CreateFromCoordinateSystems(
        GeographicCoordinateSystem.WGS84,
        ProjectedCoordinateSystem.WGS84_UTM(23, true) // Zona 23, Hemisfério Sul
    ).MathTransform;
}
```

### UTM para WGS84

```csharp
private MathTransform GetUtmToWgs84()
{
    return _ctFactory.CreateFromCoordinateSystems(
        ProjectedCoordinateSystem.WGS84_UTM(23, true),
        GeographicCoordinateSystem.WGS84
    ).MathTransform;
}
```

### Transformação de Polígonos

```csharp
private Polygon TransformPolygon(Polygon polygon, MathTransform transform)
{
    var transformedCoords = polygon.Coordinates
        .Select(c => {
            var transformed = transform.Transform(new[] { c.X, c.Y });
            return new Coordinate(transformed[0], transformed[1]);
        })
        .ToArray();

    return _geometryFactory.CreatePolygon(transformedCoords);
}
```

---

## Validação e Correção de Geometrias

### Por que Validar?

Operações geométricas podem criar geometrias topologicamente inválidas:

- Auto-interseções
- Vértices duplicados
- Anéis não fechados
- Orientação incorreta

### Método de Validação

```csharp
private Geometry? ValidateAndFixGeometry(Geometry? geometry)
{
    if (geometry == null || geometry.IsEmpty)
        return geometry;

    try
    {
        // Se já é válida, retorna
        if (geometry.IsValid)
            return geometry;

        // Tentativa 1: Buffer(0) - técnica comum para corrigir topologia
        var fixed1 = geometry.Buffer(0);
        if (fixed1.IsValid)
            return fixed1;

        // Tentativa 2: Normalizar + Buffer
        var normalized = (Geometry)geometry.Copy();
        normalized.Normalize();
        var fixed2 = normalized.Buffer(0);
        if (fixed2.IsValid)
            return fixed2;

        // Tentativa 3: Simplificar + Buffer
        var simplified = DouglasPeuckerSimplifier.Simplify(geometry, 0.0001);
        var fixed3 = simplified.Buffer(0);
        if (fixed3.IsValid)
            return fixed3;

        // Fallback: retorna o melhor resultado possível
        return fixed1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao validar geometria: {ex.Message}");
        return geometry.Buffer(0);
    }
}
```

### Técnicas de Correção

1. **Buffer(0)**: Remove auto-interseções e corrige topologia
2. **Normalize()**: Padroniza orientação e ordem de vértices
3. **DouglasPeucker**: Simplifica geometria removendo vértices redundantes

---

## Formato de Saída (GeoJSON)

### Estrutura do GeoJSON

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "properties": {
        "type": "hexagon",
        "id": 1
      },
      "geometry": {
        "type": "Polygon",
        "coordinates": [
          [
            [-51.394, -22.058],
            [-51.395, -22.058],
            [-51.395, -22.059],
            [-51.394, -22.059],
            [-51.394, -22.058],
            [-51.394, -22.058]
          ]
        ]
      }
    }
  ]
}
```

### Método de Conversão

```csharp
private JsonElement ConvertHexagonsToGeoJson(List<Geometry> hexagons)
{
    var transform = GetUtmToWgs84();
    var features = new List<object>();

    for (int i = 0; i < hexagons.Count; i++)
    {
        var hex = hexagons[i];

        // Transformar coordenadas de volta para WGS84
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
}
```

---

## Exemplo de Uso

### Requisição HTTP

```http
POST /api/utils/generate-hexagons
Content-Type: application/json

{
  "polygon": {
    "type": "Polygon",
    "coordinates": [
      [
        [-51.394, -22.058],
        [-51.395, -22.058],
        [-51.395, -22.059],
        [-51.394, -22.059],
        [-51.394, -22.058]
      ]
    ]
  },
  "hectares": 0.5
}
```

### Controller

```csharp
[HttpPost("generate-hexagons")]
public IActionResult GenerateHexagons([FromBody] HexagonRequestDto request)
{
    try
    {
        var result = _utilsService.GenerateHexagons(request.Polygon, request.Hectares);
        return Ok(result);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

### DTOs

```csharp
public class HexagonRequestDto
{
    public JsonElement Polygon { get; set; }
    public double Hectares { get; set; }
}
```

---

## Otimizações Implementadas

### 1. PreparedGeometry

Usa `PreparedGeometryFactory` para acelerar testes de interseção:

```csharp
var preparedPolygon = PreparedGeometryFactory.Prepare(validatedPolygon);
```

### 2. Validação Antecipada

Valida e corrige geometrias antes de operações custosas:

```csharp
var validatedPolygon = ValidateAndFixGeometry(projectedPolygon);
var validatedHexagon = ValidateAndFixGeometry(hexagon);
```

### 3. Tratamento de Erros Individual

Erros em hexágonos individuais não interrompem o processo:

```csharp
try
{
    var intersection = validatedPolygon.Intersection(validatedHexagon);
    hexagons.Add(intersection);
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao processar hexágono: {ex.Message}");
    continue; // Continua com o próximo
}
```

---

## Dependências

### Bibliotecas Utilizadas

- **NetTopologySuite**: Manipulação de geometrias espaciais
- **ProjNet**: Transformações de sistemas de coordenadas
- **System.Text.Json**: Serialização JSON

### Instalação

```bash
dotnet add package NetTopologySuite
dotnet add package ProjNet
```

---

## Considerações de Performance

### Complexidade

- **Parsing**: O(n) onde n = número de vértices
- **Transformação**: O(n) onde n = número de coordenadas
- **Geração de Grid**: O(rows × cols) onde rows e cols dependem da área e tamanho do hexágono
- **Interseção**: O(h × v) onde h = número de hexágonos, v = complexidade do polígono

### Recomendações

1. **Áreas Grandes**: Considere usar hexágonos maiores para reduzir o número de células
2. **Polígonos Complexos**: Simplifique o polígono de entrada se possível
3. **Cache**: Considere cachear grids frequentemente usados

---

## Limitações Conhecidas

1. **Zona UTM Fixa**: Atualmente usa zona 23S (adequado para Brasil central/sudeste)
2. **Precisão**: Pequenas diferenças podem ocorrer devido a transformações de coordenadas
3. **Geometrias Complexas**: Polígonos com muitos buracos podem ser mais lentos
4. **Tamanhos Extremos**: Hexágonos muito pequenos (< 0.01 ha) ou muito grandes (> 100 ha) podem causar problemas

---

## Troubleshooting

### ⚠️ Erro 1: TopologyException (MAIS COMUM)

**Mensagem típica:**

```
Erro ao gerar hexágonos: found non-noded intersection between LINESTRING ...
```

**Causa:**
Durante o cálculo `validatedPolygon.Intersection(validatedHexagon)`, o algoritmo encontra vértices ou interseções degeneradas — geralmente por coordenadas muito próximas ou geometria malformada.

**Solução:**

```csharp
try
{
    // Aplicar Buffer(0) antes da interseção para corrigir topologia
    var intersection = validatedPolygon.Buffer(0).Intersection(validatedHexagon.Buffer(0));

    if (intersection != null && !intersection.IsEmpty && intersection.Area > 0)
    {
        hexagons.Add(intersection);
    }
}
catch (TopologyException tex)
{
    Console.WriteLine($"[WARN] Hexágono inválido: {tex.Message}");
    continue; // Pula este hexágono e continua
}
```

**Por que funciona:**
O `Buffer(0)` força o NetTopologySuite a reconstruir a geometria, corrigindo:

- Vértices duplicados
- Auto-interseções
- Coordenadas muito próximas
- Nós não conectados

---

### 🌍 Erro 2: Coordinate Transformation Failed

**Mensagem típica:**

```
Erro ao gerar hexágonos: latitude or longitude out of range
```

**Causa:**
O método `GetWgs84ToUtm()` usa zona UTM fixa (23S) — mas se o polígono estiver em outra zona UTM (por exemplo 22S, 24S, Norte do Brasil etc.), o ProjNet lança erro de projeção.

**Solução - Cálculo Automático da Zona UTM:**

```csharp
private MathTransform GetWgs84ToUtm(Geometry geometry)
{
    var centroid = geometry.Centroid;

    // Calcular zona UTM automaticamente baseado na longitude
    int zone = (int)Math.Floor((centroid.Coordinate.X + 180) / 6) + 1;

    // Determinar hemisfério baseado na latitude
    bool isSouth = centroid.Coordinate.Y < 0;

    return _ctFactory.CreateFromCoordinateSystems(
        GeographicCoordinateSystem.WGS84,
        ProjectedCoordinateSystem.WGS84_UTM(zone, isSouth)
    ).MathTransform;
}

// Atualizar o método principal:
public JsonElement GenerateHexagons(JsonElement polygonGeoJson, double hectares)
{
    var inputPolygon = ParsePolygon(polygonGeoJson);

    // Usar o método que calcula a zona automaticamente
    var transformToUtm = GetWgs84ToUtm(inputPolygon);
    var transformedPolygon = TransformPolygon(inputPolygon, transformToUtm);

    // ... resto do código
}
```

**Zonas UTM do Brasil:**

- Norte: Zonas 18N a 22N
- Sul: Zonas 18S a 25S
- **Zona 23S**: São Paulo, parte de MG, MS
- **Zona 22S**: Rio de Janeiro, Espírito Santo
- **Zona 24S**: Paraná, Santa Catarina

---

### 🔴 Erro 3: Object Reference Not Set (NullReferenceException)

**Mensagem típica:**

```
Object reference not set to an instance of an object
```

**Causa:**
Algum `Polygon` retornou `null` durante a interseção ou transformação — normalmente quando:

- O polígono original está vazio
- O GeoJSON não está bem formado
- A transformação de coordenadas falhou

**Solução - Validação Defensiva:**

```csharp
public JsonElement GenerateHexagons(JsonElement polygonGeoJson, double hectares)
{
    try
    {
        var inputPolygon = ParsePolygon(polygonGeoJson);

        // Validação 1: Polígono não pode ser nulo ou vazio
        if (inputPolygon == null || inputPolygon.IsEmpty)
            throw new Exception("Polígono inválido ou vazio.");

        // Validação 2: Polígono deve ter área
        if (inputPolygon.Area <= 0)
            throw new Exception("Polígono sem área válida.");

        var transformedPolygon = TransformPolygon(inputPolygon, GetWgs84ToUtm(inputPolygon));

        // Validação 3: Transformação não pode resultar em nulo
        if (transformedPolygon == null || transformedPolygon.IsEmpty)
            throw new Exception("Erro na transformação de coordenadas.");

        var hexagons = GenerateHexagonalGrid(transformedPolygon, hectares);

        return ConvertHexagonsToGeoJson(hexagons);
    }
    catch (Exception ex)
    {
        throw new Exception("Erro ao gerar hexágonos: " + ex.Message);
    }
}

// Dentro do loop de geração:
if (preparedPolygon.Intersects(hexagon))
{
    try
    {
        var validatedHexagon = ValidateAndFixGeometry(hexagon);

        // Verificar antes de calcular interseção
        if (validatedHexagon == null || validatedHexagon.IsEmpty)
            continue;

        var intersection = validatedPolygon.Intersection(validatedHexagon);

        // Verificar resultado da interseção
        if (intersection != null && !intersection.IsEmpty && intersection.Area > 0)
        {
            hexagons.Add(intersection);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao processar hexágono ({row}, {col}): {ex.Message}");
        continue;
    }
}
```

---

### 📐 Erro 4: Number Overflow ou Infinity

**Mensagem típica:**

```
Arithmetic operation resulted in an overflow
Valor era muito grande ou muito pequeno para um Double
```

**Causa:**

- O valor de `hectares` é muito pequeno → o raio `r` tende a 0
- Causa divisões numéricas instáveis
- Ocorre com áreas menores que **0.01 ha** (~100 m²)

**Solução - Validação de Limites:**

```csharp
private List<Geometry> GenerateHexagonalGrid(Polygon projectedPolygon, double hectares)
{
    // Validar limites aceitáveis
    const double MIN_HECTARES = 0.01;  // 100 m²
    const double MAX_HECTARES = 1000;  // 1 km²

    if (hectares < MIN_HECTARES)
    {
        throw new Exception($"Área muito pequena. Mínimo: {MIN_HECTARES} ha");
    }

    if (hectares > MAX_HECTARES)
    {
        throw new Exception($"Área muito grande. Máximo: {MAX_HECTARES} ha");
    }

    double areaM2 = hectares * 10000;
    double r = Math.Sqrt((2 * areaM2) / (3 * Math.Sqrt(3)));

    // Validar se o raio é um número válido
    if (double.IsNaN(r) || double.IsInfinity(r) || r <= 0)
    {
        throw new Exception("Erro no cálculo do raio do hexágono.");
    }

    // ... resto do código
}
```

**Valores Recomendados:**

- **Mínimo**: 0.05 ha (500 m²)
- **Ideal**: 0.1 a 10 ha
- **Máximo**: 100 ha (1 km²)

---

### 🗺️ Erro 5: Invalid Geometry - Self-Intersection

**Mensagem típica:**

```
Invalid geometry: Self-intersection at or near point [x, y]
Ring Self-intersection at or near point [x, y]
```

**Causa:**
O GeoJSON de entrada possui:

- Auto-interseções (polígono que se cruza)
- Vértices duplicados
- Buracos não fechados
- Ordem de vértices incorreta

Mesmo o `Buffer(0)` pode não resolver em casos severos.

**Solução - Método Robusto de Correção:**

```csharp
/// <summary>
/// Corrige geometrias com problemas topológicos severos
/// </summary>
private Geometry FixGeometry(Geometry geometry)
{
    if (geometry == null || geometry.IsEmpty)
        return geometry;

    try
    {
        // Passo 1: Tentar Buffer(0) simples
        var buffered = geometry.Buffer(0);
        if (buffered.IsValid && !buffered.IsEmpty)
            return buffered;

        // Passo 2: Simplificar preservando topologia
        var simplified = NetTopologySuite.Simplify.TopologyPreservingSimplifier
            .Simplify(geometry, 0.5);

        buffered = simplified.Buffer(0);
        if (buffered.IsValid && !buffered.IsEmpty)
            return buffered;

        // Passo 3: DouglasPeucker com tolerância maior
        simplified = NetTopologySuite.Simplify.DouglasPeuckerSimplifier
            .Simplify(geometry, 1.0);

        buffered = simplified.Buffer(0);
        if (buffered.IsValid && !buffered.IsEmpty)
            return buffered;

        // Passo 4: Última tentativa - Buffer negativo + positivo
        var negative = geometry.Buffer(-0.5);
        var positive = negative.Buffer(0.5);

        if (positive.IsValid && !positive.IsEmpty)
            return positive;

        // Se tudo falhou, retorna o melhor resultado
        return buffered;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao corrigir geometria: {ex.Message}");

        // Último recurso: retornar envelope (bounding box)
        return geometry.Envelope;
    }
}
```

**Uso no Código Principal:**

```csharp
private List<Geometry> GenerateHexagonalGrid(Polygon projectedPolygon, double hectares)
{
    // Corrigir polígono ANTES de gerar o grid
    var fixedPolygon = FixGeometry(projectedPolygon) as Polygon;

    if (fixedPolygon == null || fixedPolygon.IsEmpty)
    {
        throw new Exception("Não foi possível corrigir a geometria do polígono.");
    }

    var preparedPolygon = PreparedGeometryFactory.Prepare(fixedPolygon);

    // ... resto do código
}
```

---

### 🔧 Erro 6: Grid Sendo Cortado Entre Polígonos

**Problema:**
Ao trabalhar com múltiplas áreas (ex: floresta vs área útil), o grid é cortado incorretamente nas bordas.

**Solução - Pré-processar Polígonos:**

```csharp
public JsonElement GenerateHexagonsForMultipleAreas(
    List<JsonElement> polygonGeoJsons,
    double hectares)
{
    try
    {
        // 1. Parse todos os polígonos
        var polygons = polygonGeoJsons
            .Select(ParsePolygon)
            .Where(p => p != null && !p.IsEmpty)
            .ToList();

        // 2. Criar união de todos os polígonos
        Geometry union = polygons[0];
        for (int i = 1; i < polygons.Count; i++)
        {
            union = union.Union(polygons[i]);
        }

        // 3. Simplificar antes de transformar
        var simplified = NetTopologySuite.Simplify.DouglasPeuckerSimplifier
            .Simplify(union, 0.5);

        // 4. Transformar para UTM
        var transformedUnion = TransformPolygon(
            simplified as Polygon ?? _geometryFactory.CreatePolygon(simplified.Coordinates),
            GetWgs84ToUtm(simplified)
        );

        // 5. Gerar grid na união
        var hexagons = GenerateHexagonalGrid(transformedUnion, hectares);

        return ConvertHexagonsToGeoJson(hexagons);
    }
    catch (Exception ex)
    {
        throw new Exception("Erro ao gerar hexágonos para múltiplas áreas: " + ex.Message);
    }
}
```

---

### 📊 Checklist de Prevenção de Erros

Antes de gerar o grid, verifique:

- [ ] **GeoJSON válido**: Estrutura correta, coordenadas fechadas
- [ ] **Área adequada**: Entre 0.05 e 100 hectares
- [ ] **Polígono não vazio**: Tem área > 0
- [ ] **Sem auto-interseções**: Validar com ferramentas GIS
- [ ] **Coordenadas WGS84**: Longitude entre -180 e 180, Latitude entre -90 e 90
- [ ] **Zona UTM correta**: Calcular automaticamente ou especificar
- [ ] **Simplificar polígonos complexos**: > 1000 vértices

---

### 🛠️ Método Completo com Todas as Correções

```csharp
private List<Geometry> GenerateHexagonalGrid(Polygon projectedPolygon, double hectares)
{
    // Validações iniciais
    const double MIN_HECTARES = 0.05;
    const double MAX_HECTARES = 100;

    if (hectares < MIN_HECTARES || hectares > MAX_HECTARES)
        throw new Exception($"Área deve estar entre {MIN_HECTARES} e {MAX_HECTARES} ha");

    // Calcular dimensões
    double areaM2 = hectares * 10000;
    double r = Math.Sqrt((2 * areaM2) / (3 * Math.Sqrt(3)));
    double hexWidth = Math.Sqrt(3) * r;
    double hexHeight = 2 * r;
    double vertDist = hexHeight * 0.75;

    var bounds = projectedPolygon.EnvelopeInternal;
    var hexagons = new List<Geometry>();

    // Corrigir e validar polígono
    var validatedPolygon = FixGeometry(projectedPolygon) as Polygon;
    if (validatedPolygon == null || validatedPolygon.IsEmpty)
        throw new Exception("Polígono inválido após correção");

    var preparedPolygon = PreparedGeometryFactory.Prepare(validatedPolygon);

    // Gerar grid
    for (int row = 0; row < ((bounds.MaxY - bounds.MinY) / vertDist) + 1; row++)
    {
        for (int col = 0; col < ((bounds.MaxX - bounds.MinX) / hexWidth) + 1; col++)
        {
            try
            {
                double offset = (row % 2 == 0) ? 0 : hexWidth / 2;
                double centerX = bounds.MinX + col * hexWidth + offset;
                double centerY = bounds.MinY + row * vertDist;

                Polygon hexagon = CreateHexagon(new Coordinate(centerX, centerY), r);

                if (preparedPolygon.Intersects(hexagon))
                {
                    // Aplicar Buffer(0) antes da interseção
                    var bufferedPolygon = validatedPolygon.Buffer(0);
                    var bufferedHexagon = hexagon.Buffer(0);

                    var intersection = bufferedPolygon.Intersection(bufferedHexagon);

                    if (intersection != null && !intersection.IsEmpty && intersection.Area > 0)
                    {
                        hexagons.Add(intersection);
                    }
                }
            }
            catch (TopologyException tex)
            {
                Console.WriteLine($"[WARN] Topologia inválida em ({row},{col}): {tex.Message}");
                continue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro em ({row},{col}): {ex.Message}");
                continue;
            }
        }
    }

    return hexagons;
}
```

---

### 💡 Dicas Adicionais

**Performance:**

```csharp
// Se tiver muitos hexágonos, considere paralelização
var hexagons = new ConcurrentBag<Geometry>();

Parallel.For(0, numRows, row =>
{
    for (int col = 0; col < numCols; col++)
    {
        // ... gerar hexágono
        if (intersection != null) hexagons.Add(intersection);
    }
});
```

**Logging Detalhado:**

```csharp
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UtilsService>();

logger.LogInformation($"Gerando grid: {hectares}ha, zona UTM {zone}");
logger.LogWarning($"Hexágono inválido: {tex.Message}");
logger.LogError($"Falha crítica: {ex.Message}");
```

---

### 📞 Quando Reportar um Bug

Se após aplicar todas as correções o erro persistir, reporte com:

1. **GeoJSON completo** do polígono de entrada
2. **Tamanho do hexágono** (hectares)
3. **Mensagem de erro completa** com stack trace
4. **Coordenadas aproximadas** da área (cidade/estado)
5. **Número de vértices** do polígono

### Problemas Gerais

#### Hexágonos não estão sendo gerados

**Solução**: Verifique se:

- O polígono está em WGS84
- O polígono está fechado (primeiro ponto = último ponto)
- A área do polígono é maior que o tamanho do hexágono

#### Performance lenta

**Solução**:

- Reduza a complexidade do polígono (simplifique)
- Use hexágonos maiores
- Considere particionar áreas muito grandes
- Ative PreparedGeometry para otimizar testes de interseção

---

## Referências

- [NetTopologySuite Documentation](https://nettopologysuite.github.io/NetTopologySuite/)
- [ProjNet GitHub](https://github.com/NetTopologySuite/ProjNet4GeoAPI)
- [GeoJSON Specification](https://geojson.org/)
- [UTM Coordinate System](https://en.wikipedia.org/wiki/Universal_Transverse_Mercator_coordinate_system)
- [Hexagonal Grids in GIS](https://pro.arcgis.com/en/pro-app/latest/tool-reference/spatial-statistics/h-whyhexagons.htm)

---

**Última Atualização**: 3 de novembro de 2025
**Versão**: 1.0
**Autor**: Sistema de Coleta Agro
