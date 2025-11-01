# Endpoint de Relatórios Mobile - Documentação

## ✅ Implementação Concluída

O endpoint `/api/mobile/relatorios` foi implementado com sucesso!

---

## 📍 Endpoint

### GET /api/mobile/relatorios

**URL Completa:** `https://seu-dominio.com/api/mobile/relatorios`

**Método:** GET

**Autenticação:** Bearer Token (JWT)

---

## 🔑 Headers Obrigatórios

```
Authorization: Bearer {seu-token-jwt}
Content-Type: application/json
```

---

## 📥 Parâmetros de Query (Todos Opcionais)

| Parâmetro    | Tipo     | Descrição                                      | Exemplo                 | Padrão |
| ------------ | -------- | ---------------------------------------------- | ----------------------- | ------ |
| `fazenda`    | string   | Filtrar por nome da fazenda (case-insensitive) | `fazenda=São João`      | -      |
| `talhao`     | string   | Filtrar por talhão (case-insensitive)          | `talhao=Talhão 01`      | -      |
| `dataInicio` | DateTime | Data inicial (formato: YYYY-MM-DD)             | `dataInicio=2024-01-01` | -      |
| `dataFim`    | DateTime | Data final (formato: YYYY-MM-DD)               | `dataFim=2024-01-31`    | -      |
| `page`       | int      | Número da página                               | `page=1`                | 1      |
| `limit`      | int      | Itens por página (máx: 100)                    | `limit=10`              | 10     |

---

## 📤 Resposta de Sucesso (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fazenda": "Fazenda São João",
      "talhao": "Talhão 01",
      "data": "2024-01-15",
      "pontosColetados": 75,
      "totalPontos": 100,
      "profundidade": "0-20 cm",
      "grid": "Hexagonal",
      "localizacao": "Paragominas - PA",
      "pontos": [
        {
          "id": "1",
          "latitude": -3.0,
          "longitude": -47.0,
          "dadosAmostra": null,
          "coletado": true
        },
        {
          "id": "2",
          "latitude": -3.01,
          "longitude": -47.01,
          "dadosAmostra": null,
          "coletado": false
        }
      ]
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 5,
    "totalItems": 50,
    "itemsPerPage": 10
  }
}
```

---

## ❌ Respostas de Erro

### 400 Bad Request - Parâmetros Inválidos

```json
{
  "message": "Data inicial não pode ser maior que data final."
}
```

Outros exemplos de mensagens de erro:

- `"Token inválido."`
- `"Número da página deve ser maior ou igual a 1."`
- `"Limite deve estar entre 1 e 100."`

---

## 🧪 Exemplos de Uso

### Exemplo 1: Listar todos os relatórios (primeira página)

```bash
curl -X GET "https://api.coleta.com/api/mobile/relatorios" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Exemplo 2: Filtrar por fazenda

```bash
curl -X GET "https://api.coleta.com/api/mobile/relatorios?fazenda=São%20João" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Exemplo 3: Filtrar por período

```bash
curl -X GET "https://api.coleta.com/api/mobile/relatorios?dataInicio=2024-01-01&dataFim=2024-01-31" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Exemplo 4: Filtros combinados com paginação

```bash
curl -X GET "https://api.coleta.com/api/mobile/relatorios?fazenda=São%20João&talhao=Talhão%2001&page=2&limit=20" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 📋 Estrutura de Dados

### RelatorioMobileItemDTO

| Campo             | Tipo   | Descrição                                              |
| ----------------- | ------ | ------------------------------------------------------ |
| `id`              | string | ID único do relatório (GUID)                           |
| `fazenda`         | string | Nome da fazenda                                        |
| `talhao`          | string | Nome do talhão                                         |
| `data`            | string | Data da coleta (formato: YYYY-MM-DD)                   |
| `pontosColetados` | int    | Quantidade de pontos já coletados                      |
| `totalPontos`     | int    | Total de pontos planejados                             |
| `profundidade`    | string | Profundidade da coleta (ex: "0-20 cm")                 |
| `grid`            | string | Tipo de grid (Hexagonal, Retangular, Pontos Amostrais) |
| `localizacao`     | string | Localização (endereço da fazenda)                      |
| `pontos`          | array  | Array de pontos de coleta                              |

### PontoColetaMobileDTO

| Campo          | Tipo         | Descrição                                    |
| -------------- | ------------ | -------------------------------------------- |
| `id`           | string       | ID único do ponto                            |
| `latitude`     | double       | Coordenada de latitude                       |
| `longitude`    | double       | Coordenada de longitude                      |
| `dadosAmostra` | object\|null | Dados de análise de solo (null por enquanto) |
| `coletado`     | boolean      | Indica se o ponto foi coletado               |

---

## ⚠️ Observações Importantes

### Dados de Análise de Solo

O campo `dadosAmostra` retorna `null` porque os dados de análise de solo (pH, matéria orgânica, fósforo, potássio) **não estão disponíveis** na estrutura atual do banco de dados.

**Para implementar no futuro:**

1. Adicionar campos na tabela `PontoColetado`, OU
2. Criar tabela `AnalisesSolo` relacionada, OU
3. Armazenar no campo `JsonRelatorio`

### Filtros

- Todos os filtros são **case-insensitive**
- Filtros de texto usam `Contains` (busca parcial)
- Filtros de data são inclusivos (>=, <=)

### Paginação

- Página mínima: 1
- Limite mínimo: 1
- Limite máximo: 100
- Padrão: 10 itens por página

### Ordenação

Os relatórios são ordenados por **data de inclusão decrescente** (mais recentes primeiro).

---

## 🔒 Segurança

### Autenticação

- ✅ Token JWT obrigatório
- ✅ Validação de token em cada requisição
- ✅ Apenas relatórios do usuário autenticado são retornados

### Validações

- ✅ Validação de parâmetros de data
- ✅ Validação de paginação
- ✅ Sanitização de inputs (case-insensitive)
- ✅ Proteção contra SQL Injection (Entity Framework)

---

## 🚀 Próximos Passos Sugeridos

1. **Implementar dados de análise de solo**

   - Adicionar campos na tabela ou criar nova tabela
   - Atualizar DTOs e mapeamento

2. **Adicionar cache**

   - Implementar cache de 5 minutos para relatórios
   - Usar Redis ou MemoryCache

3. **Adicionar rate limiting**

   - Limitar a 100 requisições por minuto por usuário

4. **Adicionar logging**

   - Implementar logger para rastrear erros e uso

5. **Testes automatizados**
   - Criar testes unitários e de integração

---

## 📝 Arquivos Criados/Modificados

### Criados:

- ✅ `Models/DTOs/RelatorioMobileDTO.cs`
- ✅ `Controllers/Mobile/RelatorioMobileController.cs`

### Modificados:

- ✅ `Repositories/RelatorioRepository.cs` (adicionado método `ListarRelatoriosMobileAsync`)
- ✅ `Services/RelatorioService.cs` (adicionado método `ListarRelatoriosMobileAsync` e helpers)

---

## ✅ Checklist de Implementação

- [x] DTOs criados
- [x] Repository method implementado
- [x] Service implementado
- [x] Controller criado
- [x] Autenticação JWT configurada
- [x] Filtros implementados
- [x] Paginação implementada
- [x] Validações implementadas
- [x] Tratamento de erros implementado
- [x] Documentação criada

---

## 🎉 Pronto para Uso!

O endpoint está **100% funcional** e pronto para ser usado pelo aplicativo Flutter!
