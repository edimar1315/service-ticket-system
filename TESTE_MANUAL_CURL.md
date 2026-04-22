# TESTE MANUAL VIA CURL - CRIAR TICKET

## Passo 1: Fazer Login e Obter Token

```bash
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joaosilva@gmail.com",
    "password": "D11m12gc"
  }'
```

**Resposta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "seu_email@example.com",
  "fullName": "Seu Nome",
  "expiresAt": "2025-01-20T18:00:00Z"
}
```

**Copie o token da resposta!**

---

## Passo 2: Criar Ticket (com Token)

```bash
curl -X POST http://localhost:8081/api/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "clientName": "João Silva",
    "problemDescription": "Sistema apresenta erro ao fazer login. Testei em Chrome e Firefox.",
    "priority": 3
  }'
```

**Resposta esperada (201 Created):**
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "clientName": "João Silva",
  "problemDescription": "Sistema apresenta erro ao fazer login. Testei em Chrome e Firefox.",
  "status": 0,
  "priority": 3,
  "createdAt": "2025-01-20T10:00:00Z",
  "updatedAt": null
}
```

---

## Passo 3: Listar Tickets

```bash
curl -X GET http://localhost:8081/api/tickets \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

**Resposta esperada:**
```json
{
  "data": [
    {
      "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "clientName": "João Silva",
      "problemDescription": "Sistema apresenta erro ao fazer login. Testei em Chrome e Firefox.",
      "status": 0,
      "priority": 3,
      "createdAt": "2025-01-20T10:00:00Z",
      "updatedAt": null
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

## Valores de Prioridade

| Valor | Descrição   |
|-------|-------------|
| 1     | Low         |
| 2     | Medium      |
| 3     | High        |
| 4     | Critical    |

---

## Valores de Status

| Valor | Descrição   |
|-------|-------------|
| 0     | Pending     |
| 1     | InProgress  |
| 2     | Resolved    |
| 3     | Closed      |

---

## Erros Comuns

### Erro 400 - Bad Request
```json
{
  "message": "A descrição deve ter entre 10 e 1000 caracteres."
}
```
**Solução**: Verifique se `problemDescription` tem pelo menos 10 caracteres.

### Erro 401 - Unauthorized
```json
{
  "message": "Unauthorized"
}
```
**Solução**: Verifique se o token está correto e não expirou.

### Erro 400 - Validation Error
```json
{
  "errors": {
    "ClientName": ["O nome do cliente é obrigatório."],
    "ProblemDescription": ["A descrição do problema é obrigatória."],
    "Priority": ["A prioridade é obrigatória."]
  }
}
```
**Solução**: Envie todos os campos obrigatórios.

---

## PowerShell (Windows)

Se preferir usar PowerShell:

```powershell
# Login
$loginResponse = Invoke-RestMethod -Uri "http://localhost:8081/api/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"seu_email@example.com","password":"sua_senha"}'

$token = $loginResponse.token

# Criar Ticket
Invoke-RestMethod -Uri "http://localhost:8081/api/tickets" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{"Authorization"="Bearer $token"} `
  -Body '{"clientName":"João Silva","problemDescription":"Sistema apresenta erro ao fazer login. Testei em Chrome e Firefox.","priority":3}'
```

---

## Teste no Swagger

1. Acesse: `http://localhost:8081/swagger`
2. Clique em **"Authorize"** (cadeado verde)
3. Faça login via `/api/Auth/login`
4. Copie o token da resposta
5. Cole no formato: `Bearer {token}`
6. Clique em "Authorize"
7. Teste `POST /api/Tickets`

---

## Validação dos Campos

### ClientName
- ✅ Obrigatório
- ✅ Mínimo: 3 caracteres
- ✅ Máximo: 200 caracteres

### ProblemDescription
- ✅ Obrigatório
- ✅ Mínimo: 10 caracteres
- ✅ Máximo: 1000 caracteres

### Priority
- ✅ Obrigatório
- ✅ Deve ser: 1, 2, 3 ou 4
- ✅ Tipo: número inteiro

---

## Exemplo Completo (Sucesso)

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}' \
  | grep -o '"token":"[^"]*"' \
  | cut -d'"' -f4)

# 2. Criar Ticket
curl -X POST http://localhost:8081/api/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "clientName": "Maria Santos",
    "problemDescription": "Não consigo acessar o dashboard. Aparece erro 500.",
    "priority": 2
  }'

# 3. Listar Tickets
curl -X GET http://localhost:8081/api/tickets \
  -H "Authorization: Bearer $TOKEN"
```
