# API Mobile: Coleta Relatório

Endpoint base: `/api/mobile/coleta-relatorio`

Controller para consulta de relatórios de coleta de solo no aplicativo mobile. Requer autenticação JWT.

---

## Endpoints

### 1. Listar Relatórios

```
GET /api/mobile/coleta-relatorio/relatorios
```

Lista todos os relatórios do usuário autenticado.

#### Headers

| Header | Valor |
|--------|-------|
| Authorization | Bearer `<jwt_token>` |

#### Resposta de Sucesso (200)

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "coletaId": "7a1b2c3d-4e5f-6789-abcd-ef0123456789",
    "linkBackup": "https://storage.example.com/relatorios/arquivo.json",
    "dataInclusao": "2025-12-06T10:30:00Z",
    "nomeColeta": "Coleta Talhão A - Safra 2024",
    "talhao": "Talhão A",
    "tipoColeta": "Grade Regular",
    "fazenda": "Fazenda São João",
    "nomeCliente": "João Silva",
    "safra": "Safra 2024/2025",
    "funcionario": "Carlos Técnico",
    "observacao": "Coleta realizada após período de seca",
    "profundidade": "0-20cm",
    "tiposAnalise": ["Química", "Física", "Micronutrientes"],
    "jsonRelatorio": null,
    "isRelatorio": true
  }
]
```

#### Resposta de Erro (400)

```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

---

### 2. Obter Relatório Completo

```
GET /api/mobile/coleta-relatorio/relatorio/{relatorioId}
```

Obtém os dados completos de um relatório específico, incluindo dados geoespaciais, classificação de nutrientes e estatísticas para gráficos.

#### Headers

| Header | Valor |
|--------|-------|
| Authorization | Bearer `<jwt_token>` |

#### Parâmetros de Rota

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| relatorioId | GUID | Sim | ID do relatório ou ID da coleta |

#### Resposta de Sucesso (200)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "coletaId": "7a1b2c3d-4e5f-6789-abcd-ef0123456789",
  "linkBackup": "https://storage.example.com/relatorios/arquivo.json",
  "dataInclusao": "2025-12-06T10:30:00Z",
  "nomeColeta": "Coleta Talhão A - Safra 2024",
  "talhao": "Talhão A",
  "tipoColeta": "Grade Regular",
  "fazenda": "Fazenda São João",
  "nomeCliente": "João Silva",
  "safra": "Safra 2024/2025",
  "funcionario": "Carlos Técnico",
  "observacao": "Coleta após período de seca",
  "profundidade": "0-20cm",
  "tiposAnalise": ["Química", "Física"],
  "jsonRelatorio": "{...}",
  "isRelatorio": true,
  "dadosColeta": {
    "coletaId": "7a1b2c3d-4e5f-6789-abcd-ef0123456789",
    "geojson": {
      "type": "FeatureCollection",
      "features": []
    },
    "talhao": {
      "type": "Polygon",
      "coordinates": []
    },
    "usuarioResp": {
      "nome": "Carlos Técnico",
      "email": "carlos@example.com"
    },
    "geoJsonID": "abc123-def456-7890",
    "usuarioRespID": "user-789-abc",
    "zonas": 5,
    "areaHa": 125.50
  },
  "nutrientesClassificados": [
    {
      "ponto": 1,
      "pH": { "valor": 5.8, "classificacao": "Adequado", "cor": "#4CAF50" },
      "P": { "valor": 12.5, "classificacao": "Médio", "cor": "#FFEB3B" },
      "K": { "valor": 0.35, "classificacao": "Alto", "cor": "#2196F3" }
    }
  ],
  "estatisticasAtributos": {
    "pH": {
      "nome": "pH",
      "valores": [5.2, 5.5, 5.8, 6.0, 5.7],
      "minimo": 5.2,
      "media": 5.64,
      "maximo": 6.0,
      "classificacao": "Adequado",
      "cor": "#4CAF50",
      "quantidadePontos": 5,
      "intervaloAdequado": {
        "min": 5.5,
        "max": 6.5
      }
    },
    "P": {
      "nome": "Fósforo",
      "valores": [8.0, 12.5, 15.0, 10.2, 11.8],
      "minimo": 8.0,
      "media": 11.5,
      "maximo": 15.0,
      "classificacao": "Médio",
      "cor": "#FFEB3B",
      "quantidadePontos": 5,
      "intervaloAdequado": {
        "min": 12.0,
        "max": 30.0
      }
    },
    "V%": {
      "nome": "Saturação por Bases",
      "valores": [45.0, 52.0, 58.0, 48.5, 55.2],
      "minimo": 45.0,
      "media": 51.74,
      "maximo": 58.0,
      "classificacao": "Médio",
      "cor": "#FFEB3B",
      "quantidadePontos": 5,
      "intervaloAdequado": {
        "min": 50.0,
        "max": 70.0
      }
    }
  }
}
```

#### Resposta de Erro (400)

```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

#### Resposta de Erro (404)

```json
{
  "message": "Relatório não encontrado."
}
```

---

## Estruturas de Dados

### RelatorioOuputDTO (Lista)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | GUID | ID do relatório |
| coletaId | string | ID da coleta associada |
| linkBackup | string | URL do arquivo de backup |
| dataInclusao | DateTime | Data de criação |
| nomeColeta | string | Nome da coleta |
| talhao | string | Nome do talhão |
| tipoColeta | string | Tipo de coleta (Grade Regular, etc.) |
| fazenda | string | Nome da fazenda |
| nomeCliente | string | Nome do cliente |
| safra | string | Nome da safra |
| funcionario | string | Nome do funcionário responsável |
| observacao | string | Observações da coleta |
| profundidade | string | Profundidade da coleta (ex: 0-20cm) |
| tiposAnalise | string[] | Lista de tipos de análise |
| jsonRelatorio | string? | JSON do relatório (opcional) |
| isRelatorio | bool | Indica se é um relatório |

### RelatorioCompletoOutputDTO (Detalhe)

Inclui todos os campos de `RelatorioOuputDTO` mais:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| dadosColeta | ColetaDadosDTO | Dados geoespaciais da coleta |
| nutrientesClassificados | object[] | Classificação ponto a ponto |
| estatisticasAtributos | Dictionary | Estatísticas para histogramas |

### ColetaDadosDTO

| Campo | Tipo | Descrição |
|-------|------|-----------|
| coletaId | GUID | ID da coleta |
| geojson | object | GeoJSON com pontos coletados |
| talhao | object | Polígono do talhão |
| usuarioResp | object | Dados do usuário responsável |
| geoJsonID | GUID | ID do GeoJSON |
| usuarioRespID | GUID | ID do usuário responsável |
| zonas | int | Número de zonas de manejo |
| areaHa | decimal? | Área em hectares |

### EstatisticaAtributoDTO

| Campo | Tipo | Descrição |
|-------|------|-----------|
| nome | string | Nome do atributo |
| valores | double[] | Array de valores para histograma |
| minimo | double | Menor valor encontrado |
| media | double | Valor médio |
| maximo | double | Maior valor encontrado |
| classificacao | string | Classificação baseada na média |
| cor | string | Cor hexadecimal da classificação |
| quantidadePontos | int | Quantidade de pontos válidos |
| intervaloAdequado | object | Intervalo min/max adequado |

### IndicadorDTO

| Campo | Tipo | Descrição |
|-------|------|-----------|
| valorMedio | double | Valor médio do indicador |
| classificacao | string | Classificação textual |
| cor | string | Cor hexadecimal |
| intervaloAdequado | object | Intervalo adequado (min/max) |

---

## Classificações de Nutrientes

| Classificação | Cor | Hex |
|---------------|-----|-----|
| Muito Baixo | Vermelho | #F44336 |
| Baixo | Laranja | #FF9800 |
| Médio | Amarelo | #FFEB3B |
| Adequado | Verde | #4CAF50 |
| Alto | Azul | #2196F3 |
| Muito Alto | Roxo | #9C27B0 |

---

## Atributos Analisados

### Acidez
- pH

### Saturação
- m% (Saturação por Alumínio)
- V% (Saturação por Bases)

### Equilíbrio de Bases
- Ca/Mg
- Ca/K
- Mg/K

### Participação na CTC
- Ca/CTC
- Mg/CTC
- K/CTC
- H+Al/CTC
- Al/CTC

### Macronutrientes
- Ca (Cálcio)
- Mg (Magnésio)
- K (Potássio)
- Ca+Mg
- H+Al
- Al (Alumínio)
- P (Fósforo)
- CTC
- SB (Soma de Bases)
- MO (Matéria Orgânica)

### Micronutrientes
- Fe (Ferro)
- Cu (Cobre)
- Mn (Manganês)
- B (Boro)
- Zn (Zinco)
- S (Enxofre)

---

## Observações

1. O parâmetro `relatorioId` aceita tanto o ID do relatório quanto o ID da coleta
2. O campo `estatisticasAtributos` é utilizado para gerar histogramas no app mobile
3. O campo `nutrientesClassificados` contém a classificação ponto a ponto para mapas de calor
4. Todos os endpoints requerem token JWT válido no header Authorization
