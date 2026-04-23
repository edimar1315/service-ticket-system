# 🎫 Service Ticket System

Sistema completo de **cadastro e gerenciamento de ordens de serviço (chamados técnicos)**, desenvolvido como solução para o Teste Técnico Valid — Trust is Power.

A aplicação é composta por uma **API RESTful em .NET 8**, um **Worker Service** com mensageria via RabbitMQ, um **frontend em ReactJS** e persistência híbrida com **PostgreSQL** (relacional) e **MongoDB** (NoSQL).

---

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Estrutura do Repositório](#estrutura-do-repositório)
- [Requisitos Funcionais Implementados](#requisitos-funcionais-implementados)
- [Requisitos Técnicos Implementados](#requisitos-técnicos-implementados)
- [Padrões e Princípios Aplicados](#padrões-e-princípios-aplicados)
- [Como Executar](#como-executar)
- [Endpoints da API](#endpoints-da-api)
- [Autenticação e Autorização](#autenticação-e-autorização)
- [Mensageria e Worker](#mensageria-e-worker)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Testes](#testes)
- [Diferenciais Implementados](#diferenciais-implementados)
- [Autor](#autor)

---

## Sobre o Projeto

O sistema permite o registro e gerenciamento de **chamados técnicos**, com funcionalidades de:

- ✅ Criação de chamados com cliente, descrição, prioridade e status
- ✅ Listagem e filtragem por status, prioridade ou cliente
- ✅ Atualização de status dos chamados
- ✅ Processamento assíncrono via fila de mensagens
- ✅ Notificação simulada (gravação em MongoDB) ao finalizar um chamado

---

## Arquitetura

O projeto segue uma arquitetura em camadas baseada nos princípios da **Clean Architecture**, com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────┐
│                  Frontend (React)                │
│           Login │ Dashboard │ Tickets            │
└────────────────────┬────────────────────────────┘
                     │ HTTP / REST
┌────────────────────▼────────────────────────────┐
│             ServiceTicket.API (.NET 8)           │
│        Controllers │ Mappings │ Services         │
└──────┬─────────────┬──────────────────┬──────────┘
       │             │                  │
┌──────▼──────┐ ┌────▼──────┐  ┌───────▼────────┐
│ ServiceTicket│ │PostgreSQL │  │   RabbitMQ     │
│   .Core      │ │(EF Core)  │  │   (Mensageria) │
│ (Domínio,    │ └───────────┘  └───────┬────────┘
│ Interfaces,  │                        │
│ Events)      │             ┌──────────▼────────┐
└──────────────┘             │ ServiceTicket     │
                             │   .Worker         │
                             │ (Consome fila,    │
                             │  grava MongoDB)   │
                             └───────────────────┘
```

---

## Tecnologias Utilizadas

### Backend

| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET / C# | 8.0 | Runtime e linguagem principal |
| ASP.NET Core | 8.0 | API RESTful |
| Entity Framework Core | 8.0 | ORM para PostgreSQL |
| Npgsql EF Core | 8.0 | Provider PostgreSQL |
| MongoDB.Driver | 3.7.1 | Banco NoSQL para notificações |
| RabbitMQ.Client | 7.2.1 | Mensageria assíncrona |
| ASP.NET Core Identity | 8.0 | Gerenciamento de usuários e roles |
| JWT Bearer | 8.0 | Autenticação por token |
| AutoMapper | 12.0.1 | Mapeamento de DTOs |
| Swashbuckle (Swagger) | 6.5.0 | Documentação da API |

### Frontend

| Tecnologia | Finalidade |
|---|---|
| React 18 + Vite | Framework e bundler |
| React Router DOM | Roteamento SPA |
| Axios | Comunicação HTTP com a API |
| Tailwind CSS | Estilização utilitária |
| Context API | Gerenciamento de estado de autenticação |

### Infraestrutura

| Tecnologia | Finalidade |
|---|---|
| Docker + Docker Compose | Orquestração do ambiente completo |
| PostgreSQL | Banco de dados relacional |
| MongoDB | Banco NoSQL para logs/notificações |
| RabbitMQ | Broker de mensagens |

---
## Estrutura do Repositório

```
service-ticket-system/
│
├── ServiceTicket.API/                # Camada de apresentação (API RESTful)
│   ├── Controllers/
│   │   ├── AuthController.cs         # Endpoints de autenticação (register/login)
│   │   └── TicketsController.cs      # CRUD de chamados técnicos
│   ├── Mappings/                     # Perfis do AutoMapper
│   ├── Models/                       # DTOs de request/response
│   ├── Services/                     # Serviços de aplicação da API
│   ├── Program.cs                    # Configuração da aplicação
│   ├── appsettings.json
│   └── Dockerfile
│
├── ServiceTicket.Core/               # Camada de domínio (regras de negócio)
│   ├── Application/                  # Casos de uso e serviços de aplicação
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Ticket.cs             # Entidade principal de chamado
│   │   │   ├── TicketClosedNotification.cs  # Entidade de notificação (NoSQL)
│   │   │   └── User.cs              # Entidade de usuário
│   │   ├── Enums/                    # Status, Prioridade etc.
│   │   └── Interfaces/               # Contratos do domínio
│   ├── Events/                       # Eventos de domínio
│   └── Interfaces/                   # Interfaces de repositórios e serviços
│
├── ServiceTicket.Infrastructure/     # Camada de infraestrutura
│   ├── Data/                         # DbContext (AppDbContext)
│   ├── Extensions/                   # Extension methods de configuração
│   ├── Messaging/                    # Publisher/Consumer RabbitMQ
│   ├── Migrations/                   # Migrations do EF Core
│   ├── NoSQL/                        # Repositório MongoDB
│   └── Repositories/                 # Implementações dos repositórios
│
├── ServiceTicket.Worker/             # Worker Service (processamento assíncrono)
│   ├── Program.cs                    # Configuração do worker host
│   └── Dockerfile
│
├── ServiceTicket.Tests/              # Testes unitários e de integração
│
├── frontend/                         # Aplicação React
│   ├── src/
│   │   ├── components/               # Componentes reutilizáveis (Button, Input, Navbar...)
│   │   ├── context/
│   │   │   └── AuthContext.jsx       # Contexto de autenticação global
│   │   ├── pages/
│   │   │   ├── Login.jsx
│   │   │   ├── Dashboard.jsx
│   │   │   ├── CreateTicket.jsx
│   │   │   ├── TicketList.jsx
│   │   │   └── TicketDetails.jsx
│   │   └── services/
│   │       ├── api.js                # Instância Axios configurada
│   │       ├── authService.js        # Serviço de autenticação
│   │       └── ticketService.js      # Serviço de chamados
│   ├── vite.config.js
│   └── tailwind.config.js
│
├── docker-compose.yml                # Orquestração completa do ambiente
├── TESTE_MANUAL_CURL.md             # Guia de testes manuais via cURL
├── TESTE_PONTA_A_PONTA_CRIAR_TICKET.md  # Guia de testes end-to-end
└── README.md
```

---

## Requisitos Funcionais Implementados

| # | Requisito | Status |
|---|---|---|
| 1 | Cadastro de chamados técnicos (cliente, descrição, prioridade, status) | ✅ |
| 2 | Listagem e filtro por status, prioridade ou cliente | ✅ |
| 3 | Atualização de status do chamado | ✅ |
| 4 | Worker que processa chamados finalizados e envia simulação de notificação (gravação em MongoDB) | ✅ |

---

## Requisitos Técnicos Implementados

| Requisito | Implementação |
|---|---|
| API RESTful com .NET 8 | `ServiceTicket.API` — ASP.NET Core 8 |
| Entity Framework Core para PostgreSQL | `ServiceTicket.Infrastructure` — Npgsql EF Core 8 |
| Mensageria com RabbitMQ | `ServiceTicket.Infrastructure/Messaging` — RabbitMQ.Client 7 |
| Worker Service escutando a fila | `ServiceTicket.Worker` — .NET Worker Host |
| Autenticação JWT + OAuth/OpenID Connect | JWT Bearer + ASP.NET Core Identity |
| Frontend ReactJS | `/frontend` — React 18 + Vite + Tailwind CSS |
| Banco Relacional PostgreSQL | Configurado via Docker e EF Core Migrations |
| Banco NoSQL MongoDB | Repositório em `ServiceTicket.Infrastructure/NoSQL` |
| Princípios SOLID | Interfaces, DI, separação de responsabilidades |
| Padrão Repository/Factory | `ServiceTicket.Infrastructure/Repositories` |
| Versionamento com Git | Histórico de commits por feature/camada |
| Docker e Docker Compose | `docker-compose.yml` na raiz |

---
## Padrões e Princípios Aplicados

### SOLID

- **S** — Single Responsibility: cada classe possui uma única responsabilidade (Controller, Repository, Service separados)
- **O** — Open/Closed: extensão via interfaces sem modificar classes concretas
- **L** — Liskov Substitution: implementações concretas respeitam contratos das interfaces
- **I** — Interface Segregation: interfaces coesas e específicas por contexto
- **D** — Dependency Inversion: dependências injetadas via `IServiceCollection`

### Design Patterns

- **Repository Pattern** — abstração do acesso a dados (PostgreSQL e MongoDB)
- **Factory Pattern** — criação de objetos de domínio
- **Event-Driven** — eventos de domínio disparados ao fechar um chamado, consumidos pelo Worker via RabbitMQ

### Arquitetura

- Clean Architecture com separação em camadas: Core → Infrastructure → API
- DTOs com AutoMapper para desacoplar camadas
- Worker Service independente da API

---

## Como Executar

### Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose instalados
- (Para execução manual) .NET 8 SDK e Node.js 18+

---

### 🐳 Com Docker (recomendado)

**1. Clone o repositório**

```bash
git clone https://github.com/edimar1315/service-ticket-system.git
cd service-ticket-system
```

**2. Suba todo o ambiente**

```bash
docker compose up --build -d
```

**Serviços disponíveis após o start:**

| Serviço | URL |
|---|---|
| API (Swagger) | http://localhost:8080/swagger |
| Frontend React | http://localhost:3000 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| PostgreSQL | localhost:5432 |
| MongoDB | localhost:27017 |

**3. Derrubar o ambiente**

```bash
docker compose down
```

**4. Derrubar removendo volumes (dados)**

```bash
docker compose down -v
```

---

### ⚙️ Execução Manual

#### Backend (API)

```bash
cd ServiceTicket.API
dotnet restore
dotnet ef database update   # Aplica as migrations no PostgreSQL
dotnet run
```

#### Worker

```bash
cd ServiceTicket.Worker
dotnet restore
dotnet run
```

#### Frontend

```bash
cd frontend
cp .env.example .env       # Configure a URL da API
npm install
npm run dev
```

---

## Endpoints da API

A documentação interativa completa está disponível via **Swagger** em `http://localhost:8080/swagger`.

### Autenticação

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Cadastro de novo usuário | Não |
| POST | `/api/auth/login` | Login e geração de JWT | Não |

### Chamados (Tickets)

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| GET | `/api/tickets` | Listar todos os chamados (com filtros) | JWT |
| GET | `/api/tickets/{id}` | Buscar chamado por ID | JWT |
| POST | `/api/tickets` | Criar novo chamado | JWT |
| PUT | `/api/tickets/{id}` | Atualizar chamado | JWT |
| PATCH | `/api/tickets/{id}/status` | Atualizar status do chamado | JWT |
| DELETE | `/api/tickets/{id}` | Remover chamado | JWT (Admin) |

**Filtros disponíveis em GET `/api/tickets`:**

```
?status=Aberto
?prioridade=Alta
?cliente=NomeDoCliente
```

---

## Autenticação e Autorização

O sistema utiliza **JWT Bearer Token** com **ASP.NET Core Identity**.

### Fluxo de autenticação

```
1. POST /api/auth/register  →  Cria usuário com hash de senha
2. POST /api/auth/login     →  Retorna JWT Token (Bearer)
3. Todos os endpoints protegidos requerem: Authorization: Bearer {token}
```

### Configuração JWT

- **Issuer**: `ServiceTicketAPI`
- **Audience**: `ServiceTicketClients`
- **Expiração**: 1 hora
- **Algoritmo**: HS256 (symmetric key)

### Roles implementadas

- **Admin**: acesso completo, incluindo deleção de chamados
- **User**: criação, listagem e atualização de chamados

---
## Mensageria e Worker

### Fluxo de Mensagens

```
1. Cliente fecha um chamado via API (PATCH /api/tickets/{id}/status)
2. API publica mensagem na fila RabbitMQ `ticket-closed-queue`
3. Worker Service consome a mensagem da fila
4. Worker processa o evento e grava notificação no MongoDB
5. Worker confirma (ACK) processamento da mensagem
```

### Configuração RabbitMQ

- **Exchange**: `ticket-events`
- **Queue**: `ticket-closed-queue`
- **Routing Key**: `ticket.closed`
- **Durabilidade**: Mensagens persistentes
- **Retry**: Máximo de 3 tentativas
- **DLQ (Dead Letter Queue)**: `ticket-closed-queue-dlq`

---

## Variáveis de Ambiente

### API (.NET)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=serviceticketdb;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "Jwt": {
    "SecretKey": "sua-chave-secreta-super-segura-aqui",
    "Issuer": "ServiceTicketAPI",
    "Audience": "ServiceTicketClients",
    "ExpirationInHours": 1
  }
}
```

### Worker (.NET)

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "serviceticket",
    "CollectionName": "notifications"
  }
}
```

### Frontend (React)

```env
VITE_API_URL=http://localhost:8080/api
```

---

## Testes

O projeto inclui:

### Testes Unitários

Localizados em `ServiceTicket.Tests/`, cobrindo:

- Serviços de domínio
- Repositórios
- Mapeamentos (AutoMapper)
- Validações de entidades

### Testes Manuais

Disponíveis em:

- **`TESTE_MANUAL_CURL.md`**: testes via cURL para todos os endpoints
- **`TESTE_PONTA_A_PONTA_CRIAR_TICKET.md`**: cenário completo end-to-end

### Executar Testes

```bash
cd ServiceTicket.Tests
dotnet test
```

---

## Diferenciais Implementados

✅ **Docker para subir o ambiente completo** (API, Worker, bancos, mensageria, frontend)
✅ **Pipeline de CI/CD** (mesmo simplificado)
✅ **Publicação na Google Cloud Platform (GCP)** configurada
✅ **README completo** com:
  - Descrição do projeto
  - Instruções de execução (Docker e manual)
  - Tecnologias usadas
  - Explicações técnicas detalhadas

---

## Estrutura de Commits

O versionamento segue uma estrutura semântica organizada:

```
feat(api): implementa endpoints CRUD de tickets
feat(worker): implementa consumo de fila com retry e DLQ
feat(frontend): configura estrutura base da aplicação React
feat(auth): implementa autenticação JWT com Identity
feat(infra): adiciona suporte ao Docker e Docker Compose
fix(api): corrige mapeamento de DTOs em TicketsController
refactor(core): aplica princípios SOLID em serviços de domínio
```

---

## Autor

**Edimar Barbosa da Silva**

- GitHub: [@edimar1315](https://github.com/edimar1315)
- LinkedIn: [Edimar Silva](https://www.linkedin.com/in/edimar-silva)
- Email: edimarbarbosasilva@gmail.com

---

## Prazo de Entrega

✅ Desafio desenvolvido dentro do prazo de **5 dias corridos**

**Repositório público no GitHub**: [https://github.com/edimar1315/service-ticket-system](https://github.com/edimar1315/service-ticket-system)

---

## Licença

Este projeto foi desenvolvido como teste técnico para a Valid e está disponível sob licença MIT.

---

**⭐ Se este projeto foi útil, deixe uma estrela no repositório!**

