# Desafio Técnico 2026 — Inventário Comercial

Solução full-stack do desafio técnico: **CRUD de Categorias e Produtos** com
relacionamento 1:N, API REST em **.NET 9** + **PostgreSQL** e interface
reativa em **Nuxt 3** + **Vue 3** + **Vuetify 3**.

> **Tema escolhido:** Catálogo de Inventário Comercial — alinhado ao Epic
> descrito no PDF do desafio.

---

## Sumário

- [Stack e arquitetura](#stack-e-arquitetura)
- [Estrutura do repositório](#estrutura-do-repositório)
- [Pré-requisitos](#pré-requisitos)
- [Subindo o ambiente local](#subindo-o-ambiente-local)
- [Endpoints da API](#endpoints-da-api)
- [Payloads de teste](#payloads-de-teste)
- [Documentação adicional](#documentação-adicional)
- [Conformidade com os Critérios de Aceite](#conformidade-com-os-critérios-de-aceite)

---

## Stack e arquitetura

| Camada      | Tecnologia                                                  |
| ----------- | ----------------------------------------------------------- |
| Backend     | .NET 9 (ASP.NET Core Web API, Controllers)                  |
| ORM         | Entity Framework Core 9 + Npgsql                            |
| Banco       | PostgreSQL 16 (rodando em Docker, **nunca InMemory**)       |
| Frontend    | Nuxt 3 (SPA) + Vue 3 (Composition API) + Vuetify 3          |
| HTTP client | `$fetch` nativo do Nuxt (ofetch) com wrapper tipado         |
| Doc API     | Swagger / OpenAPI (`/swagger`)                              |

Arquitetura monorepo com duas aplicações independentes:

```
                              ┌──────────────┐
       Browser  ──────────────►  Nuxt 3 SPA  │  (localhost:3000)
                              └──────┬───────┘
                                     │  HTTP (CORS restrito)
                                     ▼
                              ┌──────────────┐         ┌──────────────┐
                              │   .NET 9 API │ ───────►│ PostgreSQL 16│
                              │   Controllers│   EF    │  (Docker)    │
                              └──────────────┘         └──────────────┘
                                (localhost:5228)         (localhost:5435)
```

## Estrutura do repositório

```
desafio-codigo-2026/
├── api/                                  # Backend .NET 9
│   ├── NuGet.config                      # Força nuget.org (evita feeds privados)
│   ├── DesafioInventario.sln
│   └── DesafioInventario.Api/
│       ├── Controllers/
│       │   ├── CategoriasController.cs
│       │   └── ProdutosController.cs
│       ├── Data/
│       │   └── AppDbContext.cs           # Fluent API, mapeamento 1:N
│       ├── DTOs/
│       │   ├── CategoriaDtos.cs          # DataAnnotations: Required, MinLength(5)
│       │   └── ProdutoDtos.cs
│       ├── Models/
│       │   ├── Categoria.cs
│       │   └── Produto.cs
│       ├── Migrations/                   # Gerada pelo EF Core CLI
│       ├── Program.cs                    # CORS específico, DbContext, Swagger
│       ├── appsettings.json              # Connection string + origins do front
│       └── DesafioInventario.Api.csproj
├── web/                                  # Frontend Nuxt 3 + Vuetify
│   ├── app.vue
│   ├── nuxt.config.ts                    # SSR off, runtimeConfig.apiBase
│   ├── package.json
│   ├── plugins/
│   │   └── vuetify.ts                    # Tema, defaults, ícones MDI
│   ├── layouts/
│   │   └── default.vue                   # AppBar + Snackbar global
│   ├── composables/
│   │   ├── useApi.ts                     # Wrapper tipado de $fetch com ApiError
│   │   └── useFeedback.ts                # Estado global de toast
│   ├── components/
│   │   └── ConfirmDialog.vue
│   └── pages/
│       ├── index.vue                     # Redireciona para /categorias
│       ├── categorias.vue                # Grid + form + delete + toast
│       └── produtos.vue                  # Grid + form com Select + delete + toast
├── docs/
│   ├── modelagem.md                      # Diagrama ER e descrição das tabelas
│   └── prototipo.md                      # Wireframes + componentes Vuetify
├── docker-compose.yml                    # PostgreSQL 16 com healthcheck
├── .gitignore
└── README.md                             # este arquivo
```

## Pré-requisitos

| Ferramenta            | Versão usada no desenvolvimento | Como instalar                                                      |
| --------------------- | ------------------------------- | ------------------------------------------------------------------ |
| .NET SDK              | **9.0.x**                       | <https://dotnet.microsoft.com/download>                            |
| Node.js               | **20.x LTS**                    | <https://nodejs.org/>                                              |
| Docker Desktop        | qualquer recente                | <https://www.docker.com/products/docker-desktop/>                  |
| EF Core CLI           | **9.x** ou superior             | `dotnet tool install --global dotnet-ef`                           |

> Se você já tem um PostgreSQL local, pode pular o Docker — basta ajustar a
> connection string em `api/DesafioInventario.Api/appsettings.json`.

## Subindo o ambiente local

### 1. Clonar e entrar no projeto

```bash
git clone https://github.com/gustavograciano/desafio-codigo-2026.git
cd desafio-codigo-2026
```

### 2. Subir o PostgreSQL (Docker)

```bash
docker compose up -d
```

Isso cria o container `desafio-inventario-pg` ouvindo em `localhost:5435` com
o banco `inventario`, usuário e senha `inventario`. O volume `desafio_inventario_pgdata`
preserva os dados entre reinícios.

### 3. Restaurar pacotes e aplicar migrations

```bash
cd api
dotnet restore
cd DesafioInventario.Api
dotnet ef database update
```

> A primeira execução demora alguns segundos enquanto o EF baixa o provider
> e aplica a migration `InitialCreate`.

### 4. Subir a API

Continuando dentro de `api/DesafioInventario.Api/`:

```bash
dotnet run --launch-profile http
```

A API sobe em **<http://localhost:5228>** com Swagger em
**<http://localhost:5228/swagger>**.

### 5. Restaurar e subir o frontend

Em outro terminal:

```bash
cd web
npm install
npm run dev
```

O Nuxt sobe em **<http://localhost:3000>** e já está configurado para
apontar para `http://localhost:5228` via a variável `NUXT_PUBLIC_API_BASE`.

> Para apontar para outra URL, crie `web/.env` com:
> ```
> NUXT_PUBLIC_API_BASE=https://sua-api.example.com
> ```

## Endpoints da API

Base URL: `http://localhost:5228`

### Categorias

| Verbo  | Rota                       | Descrição                              | Códigos esperados |
| ------ | -------------------------- | -------------------------------------- | ----------------- |
| GET    | `/api/categorias`          | Lista todas                            | 200               |
| GET    | `/api/categorias/{id}`     | Obtém uma                              | 200, 404          |
| POST   | `/api/categorias`          | Cria                                   | 201, 400, 409     |
| PUT    | `/api/categorias/{id}`     | Atualiza                               | 200, 400, 404, 409|
| DELETE | `/api/categorias/{id}`     | Exclui (409 se possuir produtos)       | 204, 404, **409** |

### Produtos

| Verbo  | Rota                       | Descrição                                            | Códigos       |
| ------ | -------------------------- | ---------------------------------------------------- | ------------- |
| GET    | `/api/produtos`            | Lista todos (com objeto `categoria` aninhado via `.Include()`) | 200       |
| GET    | `/api/produtos/{id}`       | Obtém um                                             | 200, 404      |
| POST   | `/api/produtos`            | Cria                                                 | 201, 400      |
| PUT    | `/api/produtos/{id}`       | Atualiza                                             | 200, 400, 404 |
| DELETE | `/api/produtos/{id}`       | Exclui                                               | 204, 404      |

## Payloads de teste

### Criar categoria — sucesso

```http
POST /api/categorias
Content-Type: application/json

{
  "nome": "Eletronicos",
  "descricao": "Aparelhos e gadgets eletronicos"
}
```

**Resposta `201 Created`:**

```json
{ "id": 1, "nome": "Eletronicos", "descricao": "Aparelhos e gadgets eletronicos" }
```

### Criar categoria — nome inválido (mínimo 5 caracteres)

```http
POST /api/categorias
Content-Type: application/json

{ "nome": "abc" }
```

**Resposta `400 Bad Request`:**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nome": ["O campo Nome deve possuir no mínimo 5 caracteres."]
  }
}
```

### Criar produto

```http
POST /api/produtos
Content-Type: application/json

{
  "nome": "Notebook Dell Inspiron",
  "descricao": "Notebook 15 polegadas",
  "preco": 3500.50,
  "categoriaId": 1
}
```

**Resposta `201 Created`:**

```json
{
  "id": 1,
  "nome": "Notebook Dell Inspiron",
  "descricao": "Notebook 15 polegadas",
  "preco": 3500.50,
  "categoriaId": 1,
  "categoria": { "id": 1, "nome": "Eletronicos" }
}
```

### Listar produtos (com `.Include()` da categoria)

```http
GET /api/produtos
```

**Resposta `200 OK`:**

```json
[
  {
    "id": 1,
    "nome": "Notebook Dell Inspiron",
    "descricao": "Notebook 15 polegadas",
    "preco": 3500.50,
    "categoriaId": 1,
    "categoria": { "id": 1, "nome": "Eletronicos" }
  }
]
```

### Tentar excluir categoria com produtos vinculados (regra de integridade)

```http
DELETE /api/categorias/1
```

**Resposta `409 Conflict`:**

```json
{
  "mensagem": "Não é possível excluir uma categoria que possua produtos vinculados."
}
```

Mensagem **idêntica à exigida no Acceptance Criteria 04 da User Story 1**.

## Documentação adicional

- [`docs/modelagem.md`](docs/modelagem.md) — diagrama ER em Mermaid, descrição
  das tabelas e decisões de projeto (FK `ON DELETE RESTRICT`, índice único,
  precisão monetária).
- [`docs/prototipo.md`](docs/prototipo.md) — wireframes ASCII de cada tela,
  fluxo de navegação em Mermaid e mapa de componentes Vuetify.

## Conformidade com os Critérios de Aceite

### User Story 1 — Backend

| AC | Critério                                           | Implementação                                                                                                |
| -- | -------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| 01 | EF Core + banco relacional permanente              | Postgres 16 via Docker; provider `Npgsql.EntityFrameworkCore.PostgreSQL`. **Nunca usa InMemory**.            |
| 02 | Modelagem relacional Categoria 1:N Produto         | Fluent API em [`AppDbContext.cs`](api/DesafioInventario.Api/Data/AppDbContext.cs) com FK explícita.          |
| 03 | Contrato exato dos endpoints                       | [`CategoriasController.cs`](api/DesafioInventario.Api/Controllers/CategoriasController.cs) e [`ProdutosController.cs`](api/DesafioInventario.Api/Controllers/ProdutosController.cs). `GET /api/produtos` usa `.Include(p => p.Categoria)`. |
| 04 | DELETE categoria com produtos vinculados → 409     | Verificação `AnyAsync` no controller + `OnDelete(DeleteBehavior.Restrict)` no DbContext (defesa em camadas). |
| 05 | Validação Nome ≥ 5 chars em POST/PUT               | DataAnnotations `[Required] [MinLength(5)]` nos DTOs; ASP.NET responde 400 com sumário automático.           |
| 06 | CORS específico, sem `AllowAnyOrigin()`            | `WithOrigins(...)` lendo de `appsettings.json:Cors:FrontendOrigins` em [`Program.cs`](api/DesafioInventario.Api/Program.cs). |

### User Story 2 — Frontend

| AC | Critério                                                | Implementação                                                                                          |
| -- | ------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| 01 | Composition API + `$fetch`                              | [`pages/categorias.vue`](web/pages/categorias.vue) e [`pages/produtos.vue`](web/pages/produtos.vue) usam `ref`, `reactive`, `computed`. Cliente em [`composables/useApi.ts`](web/composables/useApi.ts). |
| 02 | Coluna "Ações" com Editar e Excluir                     | `v-data-table` com slot `#item.acoes` em ambas as páginas.                                             |
| 03 | Modal de edição com PUT                                 | `v-dialog` reutilizado para criar/editar; submit dispara `api.atualizarCategoria/Produto`.             |
| 04 | Botão Salvar desabilitado quando Nome < 5               | `:disabled="!podeSalvar"` ligado a `computed(() => nome.trim().length >= 5)`.                          |
| 05 | Confirmação obrigatória antes de DELETE                 | [`components/ConfirmDialog.vue`](web/components/ConfirmDialog.vue) — `v-dialog` `persistent`.          |
| 06 | Sincronização reativa sem refresh                       | Após sucesso, mutação local com `splice` / `filter` / `push`. Nunca usa `window.location.reload()`.    |
| 07 | Toast com mensagem do servidor em caso de erro          | `ApiError` extrai `mensagem` do payload; [`useFeedback`](web/composables/useFeedback.ts) exibe `v-snackbar`. |
| 08 | Select de categorias alimentado por `GET /api/categorias` | `v-select` com `items="categorias"`, populado on-mount e on-open do form de produtos.                  |

---

## Notas de operação

### Parar o ambiente

```bash
# Frontend e API: Ctrl+C nos respectivos terminais
docker compose down              # Para o Postgres mas preserva o volume
docker compose down -v           # ⚠ Apaga TODO o banco
```

### Recriar a base do zero

```bash
docker compose down -v
docker compose up -d
cd api/DesafioInventario.Api
dotnet ef database update
```

### Trocar a connection string

Edite `api/DesafioInventario.Api/appsettings.json` → `ConnectionStrings.DefaultConnection`.

### Trocar a origem do CORS

Edite `api/DesafioInventario.Api/appsettings.json` → `Cors.FrontendOrigins`.
Aceita múltiplas URLs como array. **Nunca use `*`** — viola o AC 06.
