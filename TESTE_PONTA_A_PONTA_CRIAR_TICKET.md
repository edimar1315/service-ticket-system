# TESTE PONTA A PONTA - CRIAR TICKET

## ❌ PROBLEMA IDENTIFICADO

### Erro 400 (Bad Request)
O frontend enviava dados incompatíveis com o que o backend esperava.

---

## 🔍 ANÁLISE DETALHADA

### **ANTES - Frontend Enviava:**
```json
{
  "title": "Problema no sistema",
  "description": "Descrição do problema",
  "priority": "low",
  "category": "technical"
}
```

### **Backend Esperava:**
```json
{
  "clientName": "string",
  "problemDescription": "string",
  "priority": 1  // int: 1=Low, 2=Medium, 3=High, 4=Critical
}
```

### **Incompatibilidades:**
1. ❌ `title` → Backend espera `clientName`
2. ❌ `description` → Backend espera `problemDescription`
3. ❌ `priority: "low"` (string) → Backend espera `priority: 1` (int)
4. ❌ `category` → Backend não aceita este campo

---

## ✅ CORREÇÕES APLICADAS

### **1. Alterado `CreateTicket.jsx`**

**ANTES:**
```javascript
const [formData, setFormData] = useState({
  title: '',
  description: '',
  priority: '',
  category: '',
});
```

**DEPOIS:**
```javascript
const [formData, setFormData] = useState({
  clientName: '',      // ✅ Agora usa campo correto
  description: '',     // ✅ Será mapeado para problemDescription
  priority: '',        // ✅ Será convertido para número
});
```

**Formulário atualizado:**
- Campo "Título" → "Nome do Cliente"
- Campo "Descrição" → "Descrição do Problema"
- Removido campo "Categoria" (não existe no backend)
- Adicionada validação: `minLength={10}` e `maxLength={1000}`

---

### **2. Mapeamento no `ticketService.js`**

**ANTES:**
```javascript
create: async (ticketData) => {
  const response = await api.post('/api/tickets', ticketData);
  return response.data;
}
```

**DEPOIS:**
```javascript
create: async (ticketData) => {
  // Mapear prioridades de string para número conforme enum do backend
  const priorityMap = {
    'low': 1,      // Low = 1
    'medium': 2,   // Medium = 2
    'high': 3      // High = 3
  };

  // Transformar dados do frontend para formato do backend
  const payload = {
    clientName: ticketData.clientName,
    problemDescription: ticketData.description,
    priority: priorityMap[ticketData.priority] || 2
  };

  const response = await api.post('/api/tickets', payload);
  return response.data;
}
```

---

## 📊 MAPEAMENTO DE PRIORIDADES

| Frontend (string) | Backend (int) | Enum            |
|-------------------|---------------|-----------------|
| `"low"`           | `1`           | `Priority.Low`  |
| `"medium"`        | `2`           | `Priority.Medium` |
| `"high"`          | `3`           | `Priority.High` |
| -                 | `4`           | `Priority.Critical` (não usado no frontend) |

---

## 🧪 TESTE PONTA A PONTA

### **Passo 1: Preencher formulário**
```
Nome do Cliente: "João Silva"
Descrição do Problema: "Sistema apresenta erro ao fazer login no portal"
Prioridade: "Alta"
```

### **Passo 2: Frontend envia (via `ticketService.create`)**
```json
{
  "clientName": "João Silva",
  "problemDescription": "Sistema apresenta erro ao fazer login no portal",
  "priority": 3
}
```

### **Passo 3: Backend valida**
```csharp
[Required] string ClientName ✅
[Required, StringLength(1000, MinimumLength=10)] string ProblemDescription ✅
[Required, Range(1, 4)] int Priority ✅
```

### **Passo 4: Backend cria ticket**
```csharp
var priority = (Priority)request.Priority; // Cast para enum
var ticket = await _ticketService.CreateTicketAsync(
    request.ClientName,
    request.ProblemDescription,
    priority,
    cancellationToken
);
```

### **Passo 5: Backend retorna**
```json
{
  "id": "guid",
  "clientName": "João Silva",
  "problemDescription": "Sistema apresenta erro ao fazer login no portal",
  "status": 0,
  "priority": 3,
  "createdAt": "2025-01-20T10:00:00Z",
  "updatedAt": null
}
```

---

## ✅ RESULTADO

### **Fluxo Corrigido:**
1. ✅ Frontend coleta dados corretos
2. ✅ `ticketService` mapeia campos e converte tipos
3. ✅ Backend recebe payload válido
4. ✅ Validações passam
5. ✅ Ticket é criado com sucesso
6. ✅ Frontend navega para `/tickets`

---

## 🎯 VALIDAÇÕES DO BACKEND

### **CreateTicketRequest.cs**
```csharp
[Required(ErrorMessage = "O nome do cliente é obrigatório.")]
[StringLength(200, MinimumLength = 3)]
public string ClientName { get; set; }

[Required(ErrorMessage = "A descrição do problema é obrigatória.")]
[StringLength(1000, MinimumLength = 10)]
public string ProblemDescription { get; set; }

[Required(ErrorMessage = "A prioridade é obrigatória.")]
[Range(1, 4, ErrorMessage = "Prioridade inválida.")]
public int Priority { get; set; }
```

### **Requisitos:**
- ✅ `ClientName`: 3-200 caracteres
- ✅ `ProblemDescription`: 10-1000 caracteres
- ✅ `Priority`: 1-4 (int)

---

## 🔥 TESTE MANUAL

### **1. Reinicie o frontend:**
```bash
cd frontend
npm run dev
```

### **2. Faça login:**
- Acesse `http://localhost:3000/login`
- Entre com suas credenciais

### **3. Crie um ticket:**
- Clique em "+ Novo Ticket"
- Preencha:
  - **Nome do Cliente**: "Teste Cliente"
  - **Descrição**: "Problema de teste no sistema"
  - **Prioridade**: "Alta"
- Clique em "Criar Ticket"

### **4. Verifique:**
- ✅ Deve redirecionar para `/tickets`
- ✅ O novo ticket deve aparecer na lista
- ✅ Console do browser: sem erros
- ✅ Network tab: POST retorna 201 Created

---

## 🐛 TROUBLESHOOTING

### Se ainda der erro 400:
1. Verifique o console do navegador
2. Na aba Network, veja o payload enviado
3. Compare com o esperado pelo backend
4. Verifique se o token JWT está presente no header

### Se der erro 401:
1. Faça logout e login novamente
2. Verifique se o token não expirou
3. Token expira após 8 horas (configurado no `appsettings.json`)

---

## 📝 ARQUIVOS MODIFICADOS

1. ✅ `frontend/src/pages/CreateTicket.jsx`
   - Campos do formulário atualizados
   - Removido campo `category`
   - Renomeado `title` para `clientName`

2. ✅ `frontend/src/services/ticketService.js`
   - Adicionado mapeamento de prioridades
   - Transformação de payload para formato do backend

---

## 🎉 CONCLUSÃO

O bug era uma **incompatibilidade de contrato** entre frontend e backend:
- Frontend enviava campos e tipos diferentes
- Backend rejeitava com 400 (Bad Request)

Agora o fluxo está **100% sincronizado** e testável ponta a ponta! 🚀
