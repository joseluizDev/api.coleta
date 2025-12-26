# API de Relatórios - Mobile

Esta documentação descreve os endpoints disponíveis para gerenciamento de relatórios na versão mobile da API.

## Base URL
```
/api/mobile/coleta-relatorio
```

## Autenticação
Todos os endpoints requerem autenticação via token JWT no header:
```
Authorization: Bearer {token}
```

---

## Endpoints

### 1. Listar Relatórios do Usuário

Retorna a lista de todos os relatórios do usuário autenticado sem o campo `JsonRelatorio` para otimização de performance.

**Endpoint:** `GET /api/mobile/coleta-relatorio/relatorios`

**Autenticação:** Requerida

**Parâmetros:** Nenhum

**Resposta de Sucesso (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "coletaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "linkBackup": "https://storage.example.com/relatorio.pdf",
    "dataInclusao": "2025-11-18T10:30:00",
    "nomeColeta": "Coleta Talhão 1 - Safra 2025",
    "talhao": "Talhão 1",
    "tipoColeta": "Solo",
    "fazenda": "Fazenda Santa Maria",
    "nomeCliente": "João Silva",
    "safra": "Safra 2025/2026",
    "funcionario": "Maria Santos",
    "observacao": "Coleta realizada pela manhã",
    "profundidade": "0-20 cm",
    "tiposAnalise": ["Macronutrientes", "Micronutrientes"],
    "jsonRelatorio": null,
    "isRelatorio": true
  }
]
```

**Resposta de Erro (400 Bad Request):**
```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

**Campos da Resposta:**
- `id` (guid): ID único do relatório
- `coletaId` (string): ID da coleta relacionada
- `linkBackup` (string): URL do arquivo PDF do relatório
- `dataInclusao` (datetime): Data de criação do relatório
- `nomeColeta` (string): Nome da coleta
- `talhao` (string): Nome do talhão
- `tipoColeta` (string): Tipo da coleta (Solo, Foliar, etc.)
- `fazenda` (string): Nome da fazenda
- `nomeCliente` (string): Nome do cliente
- `safra` (string): Descrição ou data da safra
- `funcionario` (string): Nome do funcionário responsável
- `observacao` (string): Observações da coleta
- `profundidade` (string): Profundidade formatada da coleta
- `tiposAnalise` (array): Lista de tipos de análise realizadas
- `jsonRelatorio` (null): Sempre null neste endpoint para performance
- `isRelatorio` (boolean): Indica se possui JSON do relatório

**Exemplo de Requisição:**
```bash
curl -X GET "https://api.example.com/api/mobile/coleta-relatorio/relatorios" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### 2. Obter Relatório Completo

Retorna todos os dados do relatório incluindo o `JsonRelatorio` e informações detalhadas da coleta (mapa, grid, pontos).

**Endpoint:** `GET /api/mobile/coleta-relatorio/relatorio/{relatorioId}`

**Autenticação:** Requerida

**Parâmetros de URL:**
- `relatorioId` (guid, obrigatório): ID do relatório a ser consultado

**Resposta de Sucesso (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "coletaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "linkBackup": "https://storage.example.com/relatorio.pdf",
  "dataInclusao": "2025-11-18T10:30:00",
  "nomeColeta": "Coleta Talhão 1 - Safra 2025",
  "talhao": "Talhão 1",
  "tipoColeta": "Solo",
  "fazenda": "Fazenda Santa Maria",
  "nomeCliente": "João Silva",
  "safra": "Safra 2025/2026",
  "funcionario": "Maria Santos",
  "observacao": "Coleta realizada pela manhã",
  "profundidade": "0-20 cm",
  "tiposAnalise": ["Macronutrientes", "Micronutrientes"],
  "jsonRelatorio": "{\"analises\": [...]}",
  "isRelatorio": true,
  "dadosColeta": {
    "coletaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "geojson": {
      "grid": [
        {
          "cordenadas": [
            [-47.8345, -15.7801],
            [-47.8347, -15.7805],
            [-47.8342, -15.7807],
            [-47.8345, -15.7801]
          ]
        }
      ],
      "points": [
        {
          "dados": {
            "id": 1,
            "hexagonId": 1,
            "coletado": true
          },
          "cordenadas": [-47.8345, -15.7803]
        }
      ]
    },
    "talhao": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Talhão 1",
      "area": "150.50",
      "observacao": "Área principal",
      "coordenadas": "[{\"lat\": -15.7801, \"lng\": -47.8345}]"
    },
    "usuarioResp": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nomeCompleto": "Maria Santos",
      "cpf": "123.456.789-00",
      "email": "maria@example.com",
      "telefone": "(61) 98765-4321"
    },
    "geoJsonID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "usuarioRespID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "zonas": 1,
    "areaHa": 150.50
  }
}
```

**Resposta de Erro (400 Bad Request):**
```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

**Resposta de Erro (404 Not Found):**
```json
{
  "message": "Relatório não encontrado."
}
```

**Campos da Resposta:**

Além dos campos do endpoint de listagem, este endpoint inclui:

**`jsonRelatorio` (string):** JSON completo com os dados analíticos do relatório

**`dadosColeta` (object):** Dados completos da coleta
- `coletaId` (guid): ID da coleta
- `geojson` (object): Dados geoespaciais processados
  - `grid` (array): Polígonos das áreas mapeadas
    - `cordenadas` (array): Array de coordenadas [longitude, latitude]
  - `points` (array): Pontos de coleta
    - `dados` (object): Informações do ponto
      - `id` (integer): ID do ponto
      - `hexagonId` (integer): ID do hexágono
      - `coletado` (boolean): Se o ponto foi coletado
    - `cordenadas` (array): Coordenada [longitude, latitude]
- `talhao` (object): Informações do talhão
  - `id` (guid): ID do talhão
  - `nome` (string): Nome do talhão
  - `area` (string): Área em hectares
  - `observacao` (string): Observações
  - `coordenadas` (string): JSON com coordenadas do talhão
- `usuarioResp` (object): Dados do funcionário responsável
  - `id` (guid): ID do usuário
  - `nomeCompleto` (string): Nome completo
  - `cpf` (string): CPF
  - `email` (string): E-mail
  - `telefone` (string): Telefone
- `geoJsonID` (guid): ID do GeoJSON
- `usuarioRespID` (guid): ID do usuário responsável
- `zonas` (integer): Quantidade de polígonos/zonas
- `areaHa` (decimal): Área total em hectares

**Exemplo de Requisição:**
```bash
curl -X GET "https://api.example.com/api/mobile/coleta-relatorio/relatorio/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Fluxo de Uso Recomendado

1. **Listar Relatórios:**
   - Fazer requisição para `GET /relatorios`
   - Exibir lista resumida para o usuário
   - Armazenar os IDs dos relatórios

2. **Visualizar Detalhes:**
   - Quando usuário selecionar um relatório
   - Usar o `id` retornado na listagem
   - Fazer requisição para `GET /relatorio/{relatorioId}`
   - Exibir dados completos, incluindo mapa e análises

---

## Códigos de Status HTTP

- `200 OK`: Requisição bem-sucedida
- `400 Bad Request`: Token inválido ou parâmetros incorretos
- `401 Unauthorized`: Token não fornecido
- `404 Not Found`: Relatório não encontrado ou não pertence ao usuário
- `500 Internal Server Error`: Erro no servidor

---

## Observações

- O endpoint de listagem **não retorna** o campo `jsonRelatorio` para melhorar a performance
- Use o endpoint de detalhes apenas quando precisar do JSON completo
- Todos os relatórios retornados são filtrados pelo usuário autenticado
- As coordenadas no formato GeoJSON seguem o padrão [longitude, latitude]
- A área retorna `null` se não puder ser convertida de string para decimal

---

## Versionamento

**Versão:** 1.0  
**Data:** Novembro 2025  
**Compatibilidade:** API .NET 9.0
