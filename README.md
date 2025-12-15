# Online Survey API

> **Trabalho Acadêmico** - Projeto desenvolvido para a disciplina de Arquitetura de Software .NET do Instituto Infnet (2025).

Sistema de Questionários Online para pesquisas públicas em larga escala, desenvolvido com .NET 10.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=coverage)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=bugs)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=souluanf_OnlineSurvey&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=souluanf_OnlineSurvey)


## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) (recomendado)
- [PostgreSQL 16](https://www.postgresql.org/) (apenas para desenvolvimento local sem Docker)

## Tecnologias

- **Backend**: ASP.NET Core Minimal APIs
- **Frontend**: Blazor WebAssembly
- **Banco de Dados**: PostgreSQL 16
- **ORM**: Entity Framework Core
- **Validação**: FluentValidation
- **Containerização**: Docker

## Estrutura do Projeto

```
OnlineSurvey/
├── src/
│   ├── OnlineSurvey.Domain/          # Entidades e Enums
│   ├── OnlineSurvey.Application/     # DTOs, Services, Validators
│   ├── OnlineSurvey.Infrastructure/  # EF Core, Repositories
│   ├── OnlineSurvey.Api/             # Minimal APIs, Middlewares
│   └── OnlineSurvey.Web/             # Blazor WebAssembly
├── tests/
│   └── OnlineSurvey.Api.Tests/       # Testes de Integração
├── Dockerfile                         # Container da API
├── compose.yaml                       # Orquestração Docker
└── OnlineSurvey.sln
```

## Como Executar

### Docker (Recomendado)

```bash
# Iniciar todos os serviços
docker compose up -d

# Verificar logs
docker compose logs -f

# Parar serviços
docker compose down
```

### Desenvolvimento Local

```bash
# Restaurar dependências
dotnet restore

# Executar API
dotnet run --project src/OnlineSurvey.Api

# Executar Frontend (em outro terminal)
dotnet run --project src/OnlineSurvey.Web

# Executar testes
dotnet test
```

## Acessos

| Serviço | URL |
|---------|-----|
| Frontend | http://localhost:5000 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080 |
| PostgreSQL | localhost:5432 |

## API Endpoints

### Surveys

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/surveys` | Criar pesquisa |
| GET | `/api/surveys` | Listar pesquisas (paginado) |
| GET | `/api/surveys/active` | Listar pesquisas ativas |
| GET | `/api/surveys/{id}` | Obter pesquisa por ID |
| PUT | `/api/surveys/{id}` | Atualizar pesquisa |
| POST | `/api/surveys/{id}/activate` | Ativar pesquisa |
| POST | `/api/surveys/{id}/close` | Encerrar pesquisa |
| DELETE | `/api/surveys/{id}` | Excluir pesquisa |

### Responses

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/responses` | Enviar resposta |
| GET | `/api/surveys/{id}/results` | Obter resultados |

## Variáveis de Ambiente

Copie o arquivo `.env.example` para `.env` e configure as variáveis:

```bash
cp .env.example .env
```

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `POSTGRES_DB` | Nome do banco de dados | `onlinesurvey` |
| `POSTGRES_USER` | Usuário do PostgreSQL | `postgres` |
| `POSTGRES_PASSWORD` | Senha do PostgreSQL | `postgres` |

## Testes

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## Documentação

- [Arquitetura do Sistema](docs/arquitetura.md) - Diagramas C4, justificativas técnicas e modelo de dados

## Autor

**Luan Fernandes**
- GitHub: [@souluanf](https://github.com/souluanf)
- LinkedIn: [Luan Fernandes](https://linkedin.com/in/souluanf)

## Licença

MIT
