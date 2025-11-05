# API de Geração de Pontos de Coleta

Documentação completa do endpoint de geração de pontos de coleta dentro de áreas (hexágonos).

## Índice
- [Visão Geral](#visão-geral)
- [Endpoints](#endpoints)
- [Modelos de Dados](#modelos-de-dados)
- [Códigos de Status](#códigos-de-status)
- [Exemplos de Uso](#exemplos-de-uso)
- [Algoritmos e Métodos](#algoritmos-e-métodos)

---

## Visão Geral

Este endpoint permite gerar pontos de coleta distribuídos uniformemente dentro de áreas definidas por polígonos (geralmente hexágonos gerados pelo endpoint `/api/utils/generate-hexagons`).

**Características principais:**
- Distribuição uniforme de pontos usando triangulação e relaxamento de Lloyd
- Suporte a múltiplos polígonos (FeatureCollection)
- Alocação proporcional de pontos baseada na área de cada polígono
- Geração determinística com seed opcional
- Metadados detalhados sobre o processo de geração

---

## Endpoints

### Gerar Pontos Dentro de Áreas

Gera pontos de coleta distribuídos uniformemente dentro de polígonos (hexágonos).

**Endpoint:** `POST /api/utils/get-points-area`

**Request Body:**
```json
{
  "geoJsonAreas": {
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
              [-51.39419646469525, -22.05836744330732],
              [-51.395095805989165, -22.05856526242054],
              [-51.39516966571233, -22.05825195657082],
              [-51.39469929813556, -22.05802283510112],
              [-51.39419439650906, -22.058321637943813],
              [-51.39419646469525, -22.05836744330732]
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

**Parâmetros:**
- `geoJsonAreas` (obrigatório): FeatureCollection com os polígonos onde os pontos serão gerados
- `qtdPontosNaArea` (obrigatório): Número médio de pontos a serem gerados por hexágono
- `seed` (opcional): Seed para geração determinística. Se não fornecido, usa `Environment.TickCount`

**Response:** `200 OK`
```json
[
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
      "coordinates": [-51.39469832, -22.05829456]
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
      "coordinates": [-51.39485123, -22.05820134]
    }
  }
]
```

**Nota:** A resposta retorna apenas o array de pontos para manter compatibilidade. Os metadados são calculados internamente mas não retornados no response padrão.

**Response com Erro:** `400 Bad Request`
```json
{
  "error": "Nenhum ponto foi gerado dentro da área especificada."
}
```

---

## Modelos de Dados

### PontosDentroDaAreaRequest
```csharp
{
  "geoJsonAreas": "JsonElement",      // FeatureCollection GeoJSON
  "qtdPontosNaArea": "int",           // Pontos médios por hexágono
  "seed": "int?" (nullable)           // Seed opcional para determinismo
}
```

**Detalhamento:**
- **geoJsonAreas**: Deve ser um GeoJSON válido do tipo `FeatureCollection`
  - Cada feature deve ter uma geometria do tipo `Polygon` ou `MultiPolygon`
  - As properties devem incluir um `id` único para cada hexágono
- **qtdPontosNaArea**: Valor inteiro positivo. Representa o número médio de pontos por hexágono
  - O número real de pontos por hexágono será proporcional à área
  - Hexágonos maiores recebem mais pontos
- **seed**: Inteiro opcional para garantir resultados reproduzíveis

### PontosDentroDaAreaResponse (Interno)
```csharp
{
  "points": "JsonElement",            // Array de features GeoJSON
  "meta": {
    "perHexCounts": {                 // Pontos gerados por hexágono
      "1": 5,
      "2": 3,
      "3": 7
    },
    "seedUsado": 12345,               // Seed utilizado
    "metodo": "string"                // Métodos utilizados
  }
}
```

### Estrutura de um Ponto (Feature)
```json
{
  "type": "Feature",
  "properties": {
    "type": "point",
    "id": 1,                  // ID sequencial do ponto
    "hexagonId": 1,           // ID do hexágono pai
    "coletado": false         // Status de coleta (padrão: false)
  },
  "geometry": {
    "type": "Point",
    "coordinates": [longitude, latitude]
  }
}
```

---

## Códigos de Status

| Código | Descrição | Quando ocorre |
|--------|-----------|---------------|
| 200 | OK | Pontos gerados com sucesso |
| 400 | Bad Request | Dados inválidos, GeoJSON malformado ou nenhum ponto gerado |
| 500 | Internal Server Error | Erro interno durante o processamento |

---

## Exemplos de Uso

### Exemplo 1: Gerar 5 pontos por hexágono (determinístico)

```bash
curl -X POST "https://api.exemplo.com/api/utils/get-points-area" \
  -H "Content-Type: application/json" \
  -d '{
    "geoJsonAreas": {
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
                [-51.39419646469525, -22.05836744330732],
                [-51.395095805989165, -22.05856526242054],
                [-51.39516966571233, -22.05825195657082],
                [-51.39469929813556, -22.05802283510112],
                [-51.39419439650906, -22.058321637943813],
                [-51.39419646469525, -22.05836744330732]
              ]
            ]
          }
        }
      ]
    },
    "qtdPontosNaArea": 5,
    "seed": 12345
  }'
```

### Exemplo 2: Gerar pontos em múltiplos hexágonos

```bash
curl -X POST "https://api.exemplo.com/api/utils/get-points-area" \
  -H "Content-Type: application/json" \
  -d '{
    "geoJsonAreas": {
      "type": "FeatureCollection",
      "features": [
        {
          "type": "Feature",
          "properties": {"type": "hexagon", "id": 1},
          "geometry": {
            "type": "Polygon",
            "coordinates": [[[-51.394, -22.058], [-51.395, -22.059], [-51.396, -22.058], [-51.394, -22.058]]]
          }
        },
        {
          "type": "Feature",
          "properties": {"type": "hexagon", "id": 2},
          "geometry": {
            "type": "Polygon",
            "coordinates": [[[-51.396, -22.058], [-51.397, -22.059], [-51.398, -22.058], [-51.396, -22.058]]]
          }
        }
      ]
    },
    "qtdPontosNaArea": 8
  }'
```

### Exemplo 3: Integração com geração de hexágonos

```javascript
// Passo 1: Gerar hexágonos
const hexagonsResponse = await fetch('/api/utils/generate-hexagons', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    polygon: {
      type: "FeatureCollection",
      features: [/* ... polígono da fazenda ... */]
    },
    hectares: 1.0
  })
});

const hexagons = await hexagonsResponse.json();

// Passo 2: Gerar pontos nos hexágonos
const pointsResponse = await fetch('/api/utils/get-points-area', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    geoJsonAreas: hexagons,
    qtdPontosNaArea: 5,
    seed: 42
  })
});

const points = await pointsResponse.json();
console.log(`Gerados ${points.length} pontos de coleta`);
```

---

## Algoritmos e Métodos

O sistema utiliza múltiplos algoritmos para garantir uma distribuição uniforme e robusta dos pontos:

### 1. Triangulação (Método Principal)
- Utiliza **LibTessDotNet** para triangular polígonos complexos
- Distribui pontos proporcionalmente à área de cada triângulo
- Usa coordenadas baricêntricas para posicionamento uniforme

### 2. Farthest-Point Sampling
- Seleciona pontos maximizando a distância mínima entre eles
- Penaliza pontos próximos às bordas
- Garante espaçamento uniforme

### 3. Lloyd Relaxation (Centroidal Voronoi)
- Aplica 3 iterações de relaxamento de Lloyd
- Melhora a regularidade do espaçamento
- Move pontos para centroides das células de Voronoi

### 4. Rejection Sampling (Fallback)
- Usado quando triangulação falha
- Gera pontos aleatórios no bbox e valida se estão dentro do polígono
- Limite de 10.000 tentativas

### 5. Fallback Determinístico
- Último recurso quando outros métodos falham
- Usa centroide com jitter aleatório
- Garante que sempre haverá ao menos alguns pontos

### Transformações de Coordenadas
- **WGS84 → UTM**: Para cálculos precisos de área e distância
- **UTM → WGS84**: Para retornar coordenadas geográficas
- Zona UTM calculada automaticamente baseada no centroide

### Alocação Proporcional
```
pontos_hexagono = max(1, round((área_hexagono / área_média) * qtdPontosNaArea))
```

---

## Fluxo de Processamento

```
1. Parse do GeoJSON de entrada
   ↓
2. Validação das geometrias
   ↓
3. Cálculo de zona UTM baseado no centroide
   ↓
4. Transformação WGS84 → UTM
   ↓
5. Cálculo de áreas e alocação proporcional
   ↓
6. Para cada hexágono:
   - Triangulação do polígono
   - Geração de candidatos (20x a quantidade desejada)
   - Farthest-Point Sampling
   - Lloyd Relaxation (3 iterações)
   - Fallbacks se necessário
   ↓
7. Transformação UTM → WGS84
   ↓
8. Criação das features GeoJSON com metadados
   ↓
9. Retorno do array de pontos
```

---

## Regras de Negócio

1. **Alocação Proporcional**: Hexágonos maiores recebem mais pontos proporcionalmente
2. **Mínimo de 1 Ponto**: Todo hexágono válido recebe pelo menos 1 ponto
3. **Distribuição Uniforme**: Pontos são espaçados uniformemente dentro de cada hexágono
4. **Evitar Bordas**: Algoritmo penaliza pontos muito próximos às bordas
5. **Determinismo**: Com o mesmo seed, gera sempre os mesmos pontos
6. **Robustez**: Múltiplos fallbacks garantem que pontos sempre serão gerados

---

## Limitações e Considerações

### Geometrias Suportadas
- ✅ Polygon
- ✅ MultiPolygon
- ❌ Point, LineString, etc.

### Performance
- **Pequenas áreas** (< 10 hexágonos): < 1 segundo
- **Áreas médias** (10-100 hexágonos): 1-5 segundos
- **Grandes áreas** (> 100 hexágonos): 5-30 segundos

### Precisão
- Cálculos em UTM garantem precisão métrica
- Erro de posicionamento: < 1 metro
- Distribuição uniforme com variação < 5%

### Validação de Geometrias
- Polígonos inválidos são corrigidos automaticamente usando `Buffer(0)`
- Simplificação de Douglas-Peucker para geometrias complexas
- Preservação de topologia quando possível

---

## Troubleshooting

### Problema: "Nenhum ponto foi gerado dentro da área especificada"
**Causa:** GeoJSON inválido ou polígonos muito pequenos  
**Solução:** Verifique se:
- O GeoJSON está bem formado
- Os polígonos têm área > 0
- As coordenadas estão em WGS84 (longitude, latitude)

### Problema: Poucos pontos gerados
**Causa:** Área dos hexágonos muito desigual  
**Solução:** Aumente `qtdPontosNaArea` ou use hexágonos mais uniformes

### Problema: Pontos muito próximos às bordas
**Causa:** Geometrias com problemas topológicos  
**Solução:** Simplifique as geometrias usando o parâmetro de tolerância adequado

### Problema: Tempo de resposta lento
**Causa:** Muitos hexágonos ou geometrias muito complexas  
**Solução:** 
- Reduza o número de hexágonos
- Simplifique as geometrias antes de enviar
- Use um valor menor de `qtdPontosNaArea`

---

## Integração com Mobile

Os pontos gerados podem ser usados diretamente em aplicativos móveis para:
- Navegação até pontos de coleta
- Marcação de pontos coletados
- Visualização em mapas
- Planejamento de rotas

**Exemplo de uso dos pontos:**
```javascript
// Filtrar pontos não coletados
const pontosRestantes = points.filter(p => !p.properties.coletado);

// Agrupar por hexágono
const porHexagono = points.reduce((acc, p) => {
  const hexId = p.properties.hexagonId;
  if (!acc[hexId]) acc[hexId] = [];
  acc[hexId].push(p);
  return acc;
}, {});

// Calcular progresso
const total = points.length;
const coletados = points.filter(p => p.properties.coletado).length;
const progresso = (coletados / total) * 100;
```

---

## Changelog

### v1.0 (Novembro 2025)
- ✨ Implementação inicial
- ✨ Suporte a triangulação com LibTessDotNet
- ✨ Farthest-Point Sampling para distribuição uniforme
- ✨ Lloyd Relaxation para regularização
- ✨ Alocação proporcional por área
- ✨ Transformações UTM automáticas
- ✨ Múltiplos métodos de fallback
- ✨ Seed para geração determinística
- ✨ Metadados detalhados (interno)

---

**Última atualização:** Novembro 2025  
**Versão da API:** 1.0  
**Mantido por:** Equipe de Desenvolvimento Agro
