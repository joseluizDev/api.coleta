# API de Mensagens Agendadas

Documentação completa da API de gerenciamento de mensagens agendadas (notificações push).

## Índice
- [Autenticação](#autenticação)
- [Endpoints](#endpoints)
- [Modelos de Dados](#modelos-de-dados)
- [Códigos de Status](#códigos-de-status)
- [Exemplos de Uso](#exemplos-de-uso)

---

## Autenticação

Todos os endpoints requerem autenticação via JWT Token.

**Header obrigatório:**
```
Authorization: Bearer {seu-token-jwt}
```

**Controle de Acesso:**
- Cada funcionário só pode acessar suas próprias mensagens
- O ID do funcionário é extraído automaticamente do token JWT
- Tentativas de acesso a mensagens de outros usuários resultam em erro 403 (Forbidden)

---

## Endpoints

### 1. Criar Mensagem Agendada

Cria uma nova mensagem agendada para envio futuro.

**Endpoint:** `POST /api/MensagemAgendada`

**Request Body:**
```json
{
  "titulo": "Lembrete de Coleta",
  "mensagem": "Não esqueça de realizar a coleta no talhão Norte",
  "dataHoraEnvio": "2025-10-20T14:30:00",
  "fcmToken": "token-fcm-do-dispositivo",
  "usuarioId": "guid-do-usuario-destinatario",
  "funcionarioId": "guid-do-funcionario-que-esta-criando"
}
```

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "titulo": "Lembrete de Coleta",
  "mensagem": "Não esqueça de realizar a coleta no talhão Norte",
  "dataHoraEnvio": "2025-10-20T14:30:00",
  "dataHoraEnviada": null,
  "status": 0,
  "fcmToken": "token-fcm-do-dispositivo",
  "usuarioId": "guid-do-usuario-destinatario",
  "funcionarioId": "guid-do-funcionario",
  "mensagemErro": null,
  "tentativasEnvio": 0
}
```

---

### 2. Listar Mensagens com Filtros

Busca mensagens com filtros e paginação. Retorna apenas mensagens do funcionário logado.

**Endpoint:** `GET /api/MensagemAgendada`

**Query Parameters:**
- `Status` (opcional): Filtro por status (0=Pendente, 1=Enviada, 2=Falha, 3=Cancelada)
- `UsuarioId` (opcional): Filtro por usuário destinatário
- `DataInicio` (opcional): Data/hora inicial do período
- `DataFim` (opcional): Data/hora final do período
- `Page` (opcional): Número da página (padrão: 1)
- `PageSize` (opcional): Itens por página (padrão: 50, max: 50)

**Exemplos:**
```
GET /api/MensagemAgendada
GET /api/MensagemAgendada?Status=0
GET /api/MensagemAgendada?Status=1&Page=1&PageSize=20
GET /api/MensagemAgendada?DataInicio=2025-10-01&DataFim=2025-10-31
GET /api/MensagemAgendada?UsuarioId=guid-do-usuario
```

**Response:** `200 OK`
```json
{
  "mensagens": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "titulo": "Lembrete de Coleta",
      "mensagem": "Não esqueça de realizar a coleta no talhão Norte",
      "dataHoraEnvio": "2025-10-20T14:30:00",
      "dataHoraEnviada": "2025-10-20T14:30:05",
      "status": 1,
      "fcmToken": "token-fcm",
      "usuarioId": "guid-do-usuario",
      "funcionarioId": "guid-do-funcionario",
      "mensagemErro": null,
      "tentativasEnvio": 0
    }
  ],
  "total": 42,
  "pagina": 1,
  "tamanhoPagina": 50,
  "totalPaginas": 1
}
```

---

### 3. Listar Todas as Mensagens (sem paginação)

Retorna todas as mensagens do funcionário logado sem paginação.

**Endpoint:** `GET /api/MensagemAgendada/todas`

**Response:** `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "titulo": "Lembrete de Coleta",
    "mensagem": "Não esqueça de realizar a coleta no talhão Norte",
    "dataHoraEnvio": "2025-10-20T14:30:00",
    "dataHoraEnviada": "2025-10-20T14:30:05",
    "status": 1,
    "fcmToken": "token-fcm",
    "usuarioId": "guid-do-usuario",
    "funcionarioId": "guid-do-funcionario",
    "mensagemErro": null,
    "tentativasEnvio": 0
  }
]
```

---

### 4. Buscar Mensagem por ID

Busca uma mensagem específica. Retorna 403 se não pertencer ao funcionário logado.

**Endpoint:** `GET /api/MensagemAgendada/{id}`

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "titulo": "Lembrete de Coleta",
  "mensagem": "Não esqueça de realizar a coleta no talhão Norte",
  "dataHoraEnvio": "2025-10-20T14:30:00",
  "dataHoraEnviada": "2025-10-20T14:30:05",
  "status": 1,
  "fcmToken": "token-fcm",
  "usuarioId": "guid-do-usuario",
  "funcionarioId": "guid-do-funcionario",
  "mensagemErro": null,
  "tentativasEnvio": 0
}
```

**Erros:**
- `404 Not Found`: Mensagem não existe
- `403 Forbidden`: Mensagem pertence a outro funcionário

---

### 5. Buscar Mensagens por Usuário Destinatário

Retorna todas as mensagens enviadas para um usuário específico.

**Endpoint:** `GET /api/MensagemAgendada/usuario/{usuarioId}`

**Response:** `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "titulo": "Lembrete de Coleta",
    "mensagem": "Não esqueça de realizar a coleta",
    "dataHoraEnvio": "2025-10-20T14:30:00",
    "dataHoraEnviada": "2025-10-20T14:30:05",
    "status": 1,
    "fcmToken": "token-fcm",
    "usuarioId": "guid-do-usuario",
    "funcionarioId": "guid-do-funcionario",
    "mensagemErro": null,
    "tentativasEnvio": 0
  }
]
```

---

### 6. Atualizar Mensagem

Atualiza uma mensagem pendente. Apenas mensagens com status "Pendente" podem ser editadas.

**Endpoint:** `PUT /api/MensagemAgendada/{id}`

**Request Body:**
```json
{
  "titulo": "Lembrete de Coleta Atualizado",
  "mensagem": "Nova mensagem atualizada",
  "dataHoraEnvio": "2025-10-21T15:00:00",
  "fcmToken": "novo-token-fcm",
  "usuarioId": "guid-do-usuario",
  "funcionarioId": "guid-do-funcionario"
}
```

**Response:** `200 OK`
```json
{
  "sucesso": true
}
```

**Erros:**
- `404 Not Found`: Mensagem não existe
- `403 Forbidden`: Mensagem pertence a outro funcionário
- `400 Bad Request`: Mensagem não está pendente (já foi enviada/cancelada)

---

### 7. Cancelar Mensagem

Cancela uma mensagem agendada. Altera o status para "Cancelada".

**Endpoint:** `DELETE /api/MensagemAgendada/{id}`

**Response:** `200 OK`
```json
{
  "sucesso": true
}
```

**Erros:**
- `404 Not Found`: Mensagem não existe
- `403 Forbidden`: Mensagem pertence a outro funcionário

---

### 8. Obter Estatísticas

Retorna estatísticas das mensagens do funcionário logado.

**Endpoint:** `GET /api/MensagemAgendada/estatisticas`

**Response:** `200 OK`
```json
{
  "total": 150,
  "totalPendentes": 25,
  "totalEnviadas": 100,
  "totalFalhas": 15,
  "totalCanceladas": 10
}
```

---

## Modelos de Dados

### MensagemAgendadaRequestDTO
```csharp
{
  "titulo": "string (max: 200)",
  "mensagem": "string (max: 1000)",
  "dataHoraEnvio": "DateTime (ISO 8601)",
  "fcmToken": "string (max: 500, opcional)",
  "usuarioId": "Guid (opcional)",
  "funcionarioId": "Guid (opcional)"
}
```

### MensagemAgendadaResponseDTO
```csharp
{
  "id": "Guid",
  "titulo": "string",
  "mensagem": "string",
  "dataHoraEnvio": "DateTime",
  "dataHoraEnviada": "DateTime (nullable)",
  "status": "StatusMensagem (enum)",
  "fcmToken": "string (nullable)",
  "usuarioId": "Guid (nullable)",
  "funcionarioId": "Guid (nullable)",
  "mensagemErro": "string (nullable)",
  "tentativasEnvio": "int"
}
```

### StatusMensagem (Enum)
```
0 = Pendente    - Aguardando envio
1 = Enviada     - Enviada com sucesso
2 = Falha       - Falha no envio após 3 tentativas
3 = Cancelada   - Cancelada pelo usuário
```

---

## Códigos de Status

| Código | Descrição | Quando ocorre |
|--------|-----------|---------------|
| 200 | OK | Requisição bem-sucedida |
| 400 | Bad Request | Token inválido, dados inválidos ou operação não permitida |
| 401 | Unauthorized | Token JWT não fornecido ou expirado |
| 403 | Forbidden | Tentativa de acessar mensagem de outro funcionário |
| 404 | Not Found | Mensagem não encontrada |
| 500 | Internal Server Error | Erro interno do servidor |

---

## Exemplos de Uso

### Exemplo 1: Criar e agendar mensagem para envio

```bash
curl -X POST "https://api.exemplo.com/api/MensagemAgendada" \
  -H "Authorization: Bearer seu-token-jwt" \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Reunião Amanhã",
    "mensagem": "Não esqueça da reunião às 14h",
    "dataHoraEnvio": "2025-10-20T13:00:00",
    "fcmToken": "token-fcm-dispositivo",
    "usuarioId": "guid-do-usuario",
    "funcionarioId": "guid-do-funcionario"
  }'
```

### Exemplo 2: Buscar mensagens pendentes

```bash
curl -X GET "https://api.exemplo.com/api/MensagemAgendada?Status=0&PageSize=20" \
  -H "Authorization: Bearer seu-token-jwt"
```

### Exemplo 3: Verificar estatísticas

```bash
curl -X GET "https://api.exemplo.com/api/MensagemAgendada/estatisticas" \
  -H "Authorization: Bearer seu-token-jwt"
```

### Exemplo 4: Cancelar mensagem

```bash
curl -X DELETE "https://api.exemplo.com/api/MensagemAgendada/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer seu-token-jwt"
```

---

## Fluxo de Envio Automático

As mensagens são processadas automaticamente por um **Background Job** que:

1. Executa a cada 1 minuto
2. Busca mensagens com status "Pendente" e `dataHoraEnvio <= agora`
3. Tenta enviar via OneSignal usando o `fcmToken`
4. Atualiza o status:
   - **Enviada**: Se envio bem-sucedido
   - **Falha**: Se falhar após 3 tentativas
5. Registra erros em `mensagemErro` e incrementa `tentativasEnvio`

**Nota:** O job roda automaticamente quando a aplicação está em execução. Não requer configuração adicional.

---

## Regras de Negócio

1. **Segurança**: Funcionários só acessam suas próprias mensagens
2. **Edição**: Apenas mensagens "Pendentes" podem ser editadas
3. **Cancelamento**: Mensagens podem ser canceladas a qualquer momento
4. **Reenvio**: Não há reenvio automático após 3 falhas
5. **Token FCM**: Obrigatório para envio da notificação
6. **Agendamento**: Mensagens com `dataHoraEnvio` futura serão enviadas automaticamente no horário agendado

---

## Observações

- Todas as datas/horas devem estar no formato ISO 8601 (ex: `2025-10-20T14:30:00`)
- O `funcionarioId` no request é opcional pois é extraído do token JWT
- Mensagens enviadas não podem ser editadas ou reenviadas
- O campo `tentativasEnvio` é incrementado automaticamente em caso de falha
- Após 3 tentativas falhadas, a mensagem é marcada como "Falha" permanentemente

---

**Última atualização:** Outubro 2025
**Versão da API:** 1.0
