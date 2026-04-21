# service-ticket-system

## Docker Compose

A orquestração está na raiz do repositório, no arquivo `docker-compose.yml`.

### Subir ambiente completo

```bash
docker compose up --build -d
```

Serviços disponíveis:

- API: `http://localhost:8080`
- RabbitMQ Management: `http://localhost:15672`
- PostgreSQL: `localhost:5432`
- MongoDB: `localhost:27017`

### Derrubar ambiente

```bash
docker compose down
```

### Derrubar ambiente removendo volumes

```bash
docker compose down -v