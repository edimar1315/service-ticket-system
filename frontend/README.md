# React + Vite
# Service Ticket System - Frontend

Frontend da aplicação Service Ticket System desenvolvido com React + Vite.

## 🚀 Tecnologias

- **React 18** - Biblioteca UI
- **Vite** - Build tool e dev server
- **React Router DOM** - Roteamento
- **Axios** - Cliente HTTP
- **Tailwind CSS** - Framework CSS

## 📋 Pré-requisitos

- Node.js 18+ 
- npm ou yarn

## 🔧 Instalação

```bash
# Instalar dependências
npm install

# Configurar variáveis de ambiente
cp .env.example .env
# Editar .env com as configurações necessárias
```

## 🎮 Como executar

### Desenvolvimento

```bash
npm run dev
```

A aplicação estará disponível em `http://localhost:3000`

### Build de produção

```bash
npm run build
```

### Preview do build

```bash
npm run preview
```

## 📁 Estrutura do projeto

```
frontend/
├── src/
│   ├── components/       # Componentes reutilizáveis
│   │   ├── Button.jsx
│   │   ├── Card.jsx
│   │   ├── Input.jsx
│   │   ├── Navbar.jsx
│   │   ├── PrivateRoute.jsx
│   │   └── Select.jsx
│   ├── context/          # Contextos React
│   │   └── AuthContext.jsx
│   ├── pages/            # Páginas/rotas
│   │   ├── CreateTicket.jsx
│   │   ├── Dashboard.jsx
│   │   ├── Login.jsx
│   │   ├── TicketDetails.jsx
│   │   └── TicketList.jsx
│   ├── services/         # Integrações HTTP
│   │   ├── api.js
│   │   ├── authService.js
│   │   └── ticketService.js
│   ├── App.jsx           # Componente raiz
│   ├── main.jsx          # Entry point
│   └── index.css         # Estilos globais
├── public/
├── .env.example
├── package.json
├── vite.config.js
└── tailwind.config.js
```

## 🔑 Funcionalidades

- ✅ **Autenticação** - Login com JWT
- ✅ **Dashboard** - Métricas e tickets recentes
- ✅ **Gestão de Tickets**
  - Listagem com filtros
  - Criação de novos tickets
  - Visualização de detalhes
  - Atualização de status
- ✅ **Rotas protegidas** - Autenticação obrigatória
- ✅ **UI Responsiva** - Design adaptado para mobile/desktop

## 🎨 Padrões de Código

### Componentes

- Componentes funcionais com Hooks
- Props tipadas (JSDoc quando necessário)
- Nomenclatura clara e descritiva

### Estado

- `useState` para estado local
- Context API para estado global (autenticação)
- Hooks personalizados para lógica reutilizável

### Serviços

- Centralização de chamadas HTTP
- Interceptors para autenticação e tratamento de erros
- Separação clara entre UI e lógica de integração

### Segurança

- Tokens armazenados no localStorage
- Validação de entrada de dados
- Tratamento de erros de API
- Redirecionamento automático em 401

## 📝 Scripts disponíveis

- `npm run dev` - Servidor de desenvolvimento
- `npm run build` - Build de produção
- `npm run preview` - Preview do build
- `npm run lint` - Verificação de código

## 🔗 API Backend

Este frontend consome a API REST disponível em:
- **Desenvolvimento**: `http://localhost:8081`
- **Swagger**: `http://localhost:8081/swagger`

## 📄 Licença

Este projeto é parte do Service Ticket System.