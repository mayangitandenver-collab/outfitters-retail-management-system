# Outfitters Retail Management System

ORMS is a multi-store apparel retail platform built with ASP.NET Core, PostgreSQL, and a future Windows POS client.

## Milestone 1

- .NET 9 solution using layered architecture
- ASP.NET Core minimal API
- PostgreSQL through Entity Framework Core and Npgsql
- Swagger/OpenAPI in development
- `/health` database health endpoint
- Docker Compose for API and PostgreSQL
- xUnit tests
- GitHub Actions build and test workflow

## Run with Docker

```bash
docker compose up --build
```

API: `http://localhost:8080`

Swagger: `http://localhost:8080/swagger`
