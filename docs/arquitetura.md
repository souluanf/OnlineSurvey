# Arquitetura do Sistema de Questionários Online

Este documento apresenta a arquitetura do Sistema de Questionários Online, projetado para suportar pesquisas públicas em larga escala, como pesquisas eleitorais. A solução foi desenvolvida utilizando o ecossistema .NET, priorizando escalabilidade, manutenibilidade e time-to-market.

---

## 1. Diagrama de Contexto (C4 - Nível 1)

O diagrama de contexto mostra o sistema como uma caixa preta e suas interações com usuários e sistemas externos.

```mermaid
C4Context
    title Sistema de Questionários Online - Diagrama de Contexto

    Person(respondente, "Respondente", "Pessoa que responde às pesquisas através de links em redes sociais")
    Person(admin, "Administrador", "Usuário que cria pesquisas e visualiza resultados")

    System(sistema, "Sistema de Questionários Online", "Permite criar pesquisas, coletar respostas em escala e visualizar resultados sumarizados")

    System_Ext(redes, "Redes Sociais", "Facebook, Instagram, Twitter - divulgação das pesquisas")
    System_Ext(cdn, "CDN", "Distribuição de conteúdo estático")

    Rel(respondente, sistema, "Responde pesquisas", "HTTP")
    Rel(admin, sistema, "Gerencia pesquisas e visualiza resultados", "HTTP")
    Rel(sistema, redes, "Links compartilhados")
    Rel(cdn, respondente, "Entrega conteúdo estático")
```

### Visão para Usuários de Negócio

| Ator | Descrição | Interação |
|------|-----------|-----------|
| **Respondente** | Cidadão que acessa a pesquisa através de anúncios em redes sociais | Responde perguntas de múltipla escolha |
| **Administrador** | Equipe da startup que gerencia as pesquisas | Cria pesquisas, ativa/encerra coleta, visualiza resultados |

---

## 2. Diagrama de Containers (C4 - Nível 2)

O diagrama de containers mostra as aplicações e bancos de dados que compõem o sistema.

```mermaid
C4Container
    title Sistema de Questionários Online - Diagrama de Containers

    Person(respondente, "Respondente", "Milhões de pessoas respondendo pesquisas")
    Person(admin, "Administrador", "Equipe que gerencia pesquisas")

    Container_Boundary(sistema, "Sistema de Questionários Online") {
        Container(web, "Aplicação Web", "Blazor WebAssembly", "Interface do usuário para criação de pesquisas e visualização de resultados")
        Container(api, "API REST", "ASP.NET Core Minimal APIs", "Endpoints para gerenciar pesquisas e coletar respostas")
        ContainerDb(db, "Banco de Dados", "PostgreSQL", "Armazena pesquisas, perguntas, opções e respostas")
    }

    Rel(respondente, api, "Envia respostas", "HTTP/JSON")
    Rel(admin, web, "Acessa interface", "HTTP")
    Rel(web, api, "Consome API", "HTTP/JSON")
    Rel(api, db, "Lê/Escreve dados", "TCP/SQL")
```

### Detalhamento dos Containers

| Container | Tecnologia | Justificativa |
|-----------|------------|---------------|
| **Aplicação Web** | Blazor WebAssembly | SPA moderna, executa no browser, permite desenvolvimento full-stack em C# |
| **API REST** | ASP.NET Core Minimal APIs | Alta performance, baixa latência, ideal para APIs de alto throughput |
| **Banco de Dados** | PostgreSQL | Open-source, robusto, excelente para cargas de escrita intensiva |

---

## 3. Diagrama de Componentes (C4 - Nível 3)

### 3.1 Componentes da API

```mermaid
C4Component
    title API REST - Diagrama de Componentes

    Container_Boundary(api, "API REST - ASP.NET Core") {
        Component(endpoints, "Endpoints", "Minimal APIs", "Define rotas HTTP e handlers")
        Component(services, "Services", "Application Layer", "Lógica de negócio e orquestração")
        Component(validators, "Validators", "FluentValidation", "Validação de entrada de dados")
        Component(repositories, "Repositories", "Repository Pattern", "Abstração de acesso a dados")
        Component(efcore, "DbContext", "Entity Framework Core", "ORM e mapeamento objeto-relacional")
        Component(middleware, "Middlewares", "ASP.NET Core", "Tratamento de exceções, logging")
    }

    ContainerDb(db, "PostgreSQL", "Database")

    Rel(endpoints, validators, "Valida requests")
    Rel(endpoints, services, "Invoca lógica de negócio")
    Rel(services, repositories, "Acessa dados via")
    Rel(repositories, efcore, "Usa")
    Rel(efcore, db, "SQL Queries")
```

### 3.2 Componentes do Frontend

```mermaid
C4Component
    title Aplicação Web - Diagrama de Componentes

    Container_Boundary(web, "Blazor WebAssembly") {
        Component(pages, "Pages", "Razor Components", "Páginas da aplicação (Home, Lista, Criar, Resultados)")
        Component(components, "Components", "Razor Components", "Componentes reutilizáveis (Forms, Charts, Tabs)")
        Component(services_web, "API Services", "HttpClient", "Comunicação com a API REST")
        Component(state, "State Management", "Blazor State", "Gerenciamento de estado da aplicação")
    }

    Container(api, "API REST", "ASP.NET Core")

    Rel(pages, components, "Usa")
    Rel(pages, services_web, "Consome dados via")
    Rel(pages, state, "Lê/Atualiza estado")
    Rel(services_web, api, "HTTP/JSON")
```

---

## 4. Diagrama de Implantação

```mermaid
C4Deployment
    title Diagrama de Implantação - Docker Compose

    Deployment_Node(docker, "Docker Host", "Linux/Docker") {
        Deployment_Node(network, "onlinesurvey-network", "Bridge Network") {
            Container(web_container, "web", "nginx:alpine", "Serve Blazor WASM, proxy reverso para API")
            Container(api_container, "api", "ASP.NET Core 10", "API REST containerizada")
            ContainerDb(db_container, "db", "PostgreSQL 16", "Banco de dados persistente")
        }
    }

    Rel(web_container, api_container, "Proxy /api/*", "HTTP:8080")
    Rel(api_container, db_container, "Conexão DB", "TCP:5432")
```

---

## 5. Fluxo de Dados

### 5.1 Fluxo de Criação de Pesquisa

```mermaid
sequenceDiagram
    autonumber
    participant Admin as Administrador
    participant Web as Blazor WASM
    participant API as ASP.NET Core API
    participant Val as FluentValidation
    participant EF as Entity Framework
    participant DB as PostgreSQL

    Admin->>Web: Preenche formulário de pesquisa
    Web->>API: POST /api/surveys
    API->>Val: Valida CreateSurveyRequest
    Val-->>API: Validação OK
    API->>EF: surveyService.CreateSurveyAsync()
    EF->>DB: INSERT Survey, Questions, Options
    DB-->>EF: Entidades criadas
    EF-->>API: SurveyDetailResponse
    API-->>Web: 201 Created + JSON
    Web-->>Admin: Exibe pesquisa criada
```

### 5.2 Fluxo de Resposta (Alta Escala)

```mermaid
sequenceDiagram
    autonumber
    participant R as Respondente
    participant API as ASP.NET Core API
    participant Val as FluentValidation
    participant EF as Entity Framework
    participant DB as PostgreSQL

    R->>API: POST /api/responses
    API->>Val: Valida SubmitResponseRequest
    Val-->>API: Validação OK
    API->>EF: Verifica pesquisa ativa
    EF->>DB: SELECT Survey WHERE Id = ?
    DB-->>EF: Survey (Status = Active)
    API->>EF: responseService.SubmitResponseAsync()
    EF->>DB: INSERT Response, Answers
    DB-->>EF: Response criada
    EF-->>API: ResponseConfirmation
    API-->>R: 201 Created
```

---

## 6. Modelo de Dados

```mermaid
erDiagram
    SURVEY ||--o{ QUESTION : contains
    QUESTION ||--o{ OPTION : has
    SURVEY ||--o{ RESPONSE : receives
    RESPONSE ||--o{ ANSWER : contains
    ANSWER }o--|| OPTION : selects

    SURVEY {
        guid Id PK
        string Title
        string Description
        enum Status
        datetime StartDate
        datetime EndDate
        datetime CreatedAt
    }

    QUESTION {
        guid Id PK
        guid SurveyId FK
        string Text
        int Order
        bool IsRequired
    }

    OPTION {
        guid Id PK
        guid QuestionId FK
        string Text
        int Order
    }

    RESPONSE {
        guid Id PK
        guid SurveyId FK
        datetime SubmittedAt
        string IpAddress
    }

    ANSWER {
        guid Id PK
        guid ResponseId FK
        guid QuestionId FK
        guid SelectedOptionId FK
    }
```

---

## 7. Justificativas Arquiteturais

### 7.1 Para Desenvolvedores

#### Escolha do ASP.NET Core Minimal APIs

```csharp
// Exemplo de endpoint enxuto e performático
group.MapPost("/", CreateSurvey)
    .WithName("CreateSurvey")
    .Produces<SurveyDetailResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem();
```

**Por que escolhemos:**
- **Performance**: Menor overhead que controllers MVC, ideal para APIs de alto throughput
- **Simplicidade**: Código mais enxuto, menos boilerplate
- **Produtividade**: Time-to-market reduzido (crucial para prazo das eleições)
- **Documentação automática**: Integração nativa com Swagger/OpenAPI

#### Escolha do Entity Framework Core

```csharp
// Configuração fluente e type-safe
public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.HasMany(s => s.Questions)
               .WithOne(q => q.Survey)
               .HasForeignKey(q => q.SurveyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Por que escolhemos:**
- **Produtividade**: Migrations automáticas, LINQ queries type-safe
- **Testabilidade**: Suporte a InMemory provider para testes de integração
- **Maturidade**: ORM mais utilizado no ecossistema .NET
- **Flexibilidade**: Suporta múltiplos providers (PostgreSQL, SQL Server, SQLite)

#### Escolha do FluentValidation

```csharp
public class CreateSurveyRequestValidator : AbstractValidator<CreateSurveyRequest>
{
    public CreateSurveyRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório")
            .MaximumLength(200);

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Pesquisa deve ter pelo menos uma pergunta");
    }
}
```

**Por que escolhemos:**
- **Separação de responsabilidades**: Validação desacoplada dos endpoints
- **Testabilidade**: Validators podem ser testados unitariamente
- **Mensagens customizáveis**: Feedback claro para usuários da API

#### Escolha do Blazor WebAssembly

**Por que escolhemos:**
- **Full-stack C#**: Equipe de 5 devs já conhece .NET/C#
- **Sem JavaScript**: Reduz complexidade e curva de aprendizado
- **SPA moderna**: Experiência de usuário fluida
- **Componentes reutilizáveis**: Razor Components para UI consistente

### 7.2 Para Usuários de Negócio

| Requisito de Negócio | Solução Técnica | Benefício |
|---------------------|-----------------|-----------|
| **Milhões de respostas** | PostgreSQL + Connection Pooling | Banco robusto para alta carga de escrita |
| **Disponibilidade 24/7** | Docker containers + Health checks | Fácil deploy e monitoramento |
| **Resultados em tempo real** | API REST eficiente | Agregação rápida de resultados |
| **Prazo curto (eleições)** | .NET stack unificado | Equipe já capacitada, sem curva de aprendizado |
| **Custos controlados** | PostgreSQL open-source | Sem custos de licenciamento |

---

## 8. Estratégia de Testes

### 8.1 Pirâmide de Testes

```mermaid
graph TB
    subgraph "Pirâmide de Testes"
        E2E["🔺 E2E Tests<br/>Playwright/Selenium"]
        INT["🔷 Integration Tests<br/>WebApplicationFactory + InMemory DB"]
        UNIT["🟩 Unit Tests<br/>xUnit + Moq"]
    end

    UNIT --> INT --> E2E
```

### 8.2 Testes de Integração da API

```csharp
public class SurveyEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateSurvey_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateSurveyRequest
        {
            Title = "Pesquisa Eleitoral 2026",
            Questions = [new CreateQuestionRequest { Text = "Em quem você votaria?" }]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/surveys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

**Cobertura de testes implementada:**
- ✅ 41 testes de integração passando
- ✅ Testes com banco InMemory (isolamento)
- ✅ Validação de endpoints
- ✅ Cenários de erro

### 8.3 Testes do Entity Framework

```csharp
// Configuração para testes com InMemory Database
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

**Por que InMemory para testes:**
- Execução rápida (sem I/O de disco)
- Isolamento entre testes
- Não requer container PostgreSQL

---

## 9. Conclusão

A arquitetura proposta atende aos requisitos do sistema de questionários online:

| Requisito | Atendimento |
|-----------|-------------|
| ✅ **Escala** | PostgreSQL + APIs stateless + Docker |
| ✅ **Prazo** | Stack .NET unificado (equipe já capacitada) |
| ✅ **Manutenibilidade** | Clean Architecture + Testes automatizados |
| ✅ **Custo** | Tecnologias open-source |
| ✅ **.NET Framework** | ASP.NET Core + EF Core + Blazor |

A solução é **pragmática**, focando em entregar valor no prazo das eleições, utilizando tecnologias maduras que a equipe de 5 desenvolvedores já domina.

---

**Autor:** Luan Fernandes
**Data:** Dezembro 2025
**Disciplina:** Arquitetura de Software .NET
