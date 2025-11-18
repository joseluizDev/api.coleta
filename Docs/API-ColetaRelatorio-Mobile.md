# API Coleta e Relatório - Mobile

## Descrição

Controller responsável por fornecer endpoints mobile para visualização de coletas, relatórios e configurações personalizadas com fallback.

**Rota Base:** `api/mobile/coleta-relatorio`

**Autenticação:** Todos os endpoints requerem autenticação via Bearer Token (JWT).

---

## Endpoints

### 1. Visualizar Coleta por ID

**GET** `/api/mobile/coleta-relatorio/coleta/{id}`

Retorna os detalhes completos de uma coleta específica incluindo talhão, geojson, usuário responsável e informações relacionadas.

#### Parâmetros

| Nome | Tipo | Localização | Obrigatório | Descrição |
|------|------|-------------|-------------|-----------|
| id | Guid | Route | Sim | ID da coleta |

#### Respostas

**200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeColeta": "Coleta Talhão Norte",
  "talhaoID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "talhao": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "Talhão Norte",
    "area": 150.5,
    "coordenadas": [...]
  },
  "geoJsonID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "geojson": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "pontos": "{...}",
    "grid": "..."
  },
  "usuarioRespID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "usuarioResp": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nomeCompleto": "João Silva",
    "email": "joao@email.com",
    "telefone": "(11) 98765-4321"
  },
  "tipoColeta": "Solo",
  "tipoAnalise": ["Fertilidade", "pH"],
  "profundidade": "0-20cm",
  "observacao": "Coleta realizada após chuva",
  "dataInclusao": "2025-11-17T10:30:00"
}
```

**400 Bad Request**
```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

**404 Not Found**
```json
{
  "message": "Coleta não encontrada."
}
```

---

### 2. Buscar Relatório por ID da Coleta

**GET** `/api/mobile/coleta-relatorio/relatorio/{coletaId}`

Retorna o relatório associado a uma coleta específica.

#### Parâmetros

| Nome | Tipo | Localização | Obrigatório | Descrição |
|------|------|-------------|-------------|-----------|
| coletaId | Guid | Route | Sim | ID da coleta |

#### Respostas

**200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "coletaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "linkBackup": "https://storage.example.com/relatorio.pdf",
  "jsonRelatorio": "{...}",
  "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dataInclusao": "2025-11-17T14:20:00"
}
```

**400 Bad Request**
```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

**404 Not Found**
```json
{
  "message": "Relatório não encontrado para esta coleta."
}
```

---

### 3. Buscar Configurações com Fallback

**GET** `/api/mobile/coleta-relatorio/configuracoes/fallback`

Retorna as configurações personalizadas do usuário. Se não existirem configurações personalizadas, retorna as configurações padrão do sistema.

#### Parâmetros

Nenhum parâmetro adicional além do token de autenticação.

#### Respostas

**200 OK - Configurações Personalizadas**
```json
{
  "tipo": "personalizada",
  "configuracoes": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Baixo",
      "limiteInferior": 0,
      "limiteSuperior": 10,
      "corHex": "#FF0000",
      "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Médio",
      "limiteInferior": 10,
      "limiteSuperior": 20,
      "corHex": "#FFFF00",
      "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Alto",
      "limiteInferior": 20,
      "limiteSuperior": 100,
      "corHex": "#00FF00",
      "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ]
}
```

**200 OK - Configurações Padrão (Fallback)**
```json
{
  "tipo": "padrao",
  "configuracoes": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Muito Baixo",
      "limiteInferior": 0,
      "limiteSuperior": 5,
      "corHex": "#FF0000"
    },
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Baixo",
      "limiteInferior": 5,
      "limiteSuperior": 15,
      "corHex": "#FFA500"
    },
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Médio",
      "limiteInferior": 15,
      "limiteSuperior": 25,
      "corHex": "#FFFF00"
    },
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Alto",
      "limiteInferior": 25,
      "limiteSuperior": 100,
      "corHex": "#00FF00"
    }
  ]
}
```

**400 Bad Request**
```json
{
  "message": "Token inválido ou usuário não encontrado."
}
```

---

## Lógica de Negócio

### Endpoint de Configurações com Fallback

O endpoint `configuracoes/fallback` implementa uma lógica de fallback inteligente:

1. **Primeira Tentativa**: Busca configurações personalizadas criadas pelo usuário
   - Se existirem configurações personalizadas, retorna com `tipo: "personalizada"`

2. **Fallback**: Se não houver configurações personalizadas
   - Retorna as configurações padrão do sistema com `tipo: "padrao"`

Esta abordagem garante que:
- Usuários com configurações customizadas vejam suas preferências
- Novos usuários ou usuários sem configurações personalizadas tenham acesso às configurações padrão
- A aplicação mobile sempre receba configurações válidas para exibição

---

## Códigos de Status HTTP

| Código | Descrição |
|--------|-----------|
| 200 | Requisição bem-sucedida |
| 400 | Requisição inválida (token ausente/inválido) |
| 401 | Não autorizado (sem token) |
| 404 | Recurso não encontrado |
| 500 | Erro interno do servidor |

---

## Autenticação

Todos os endpoints requerem um Bearer Token JWT válido no header:

```
Authorization: Bearer {seu-token-jwt}
```

O token é validado e o ID do usuário é extraído para garantir que apenas dados autorizados sejam retornados.

---

## Exemplo de Uso

### cURL - Visualizar Coleta

```bash
curl -X GET "https://api.exemplo.com/api/mobile/coleta-relatorio/coleta/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### cURL - Buscar Relatório

```bash
curl -X GET "https://api.exemplo.com/api/mobile/coleta-relatorio/relatorio/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### cURL - Configurações com Fallback

```bash
curl -X GET "https://api.exemplo.com/api/mobile/coleta-relatorio/configuracoes/fallback" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Observações

- Todos os endpoints validam a autenticidade do token JWT antes de processar a requisição
- Os IDs devem ser no formato GUID válido
- As respostas incluem relacionamentos completos quando disponíveis (talhão, geojson, usuário responsável)
- O endpoint de configurações sempre retorna dados válidos, seja personalizados ou padrão
- Recomenda-se verificar o campo `tipo` na resposta das configurações para identificar se são personalizadas ou padrão

---

## Versão

**Data de Criação:** 17/11/2025  
**Versão da API:** 1.0  
**Última Atualização:** 17/11/2025
