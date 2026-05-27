# TodoApp — Full-Stack To-Do Application

Full-stack To-Do application similar to Microsoft To-Do. Manage tasks with categories, search, filter, sort, and pagination.

## Tech Stack

**Backend** (4-layer clean architecture)
- .NET 8 Web API
- Entity Framework Core 8 + PostgreSQL (Npgsql)
- JWT authentication (Bearer), BCrypt password hashing
- Swagger/OpenAPI
- Dependency Injection throughout

**Frontend**
- Angular 18 (standalone components, signals)
- Tailwind CSS 3
- Reactive Forms with field-level validation messages
- HttpClient with JWT interceptor
- Lazy-loaded routes with auth guard

**Tests**
- xUnit, Moq, FluentAssertions, EF Core InMemory

**Infrastructure**
- Docker + docker-compose (PostgreSQL + .NET API + Nginx-served Angular)

## Project Structure

```
todo-app/
├── backend/
│   ├── TodoApp.sln
│   ├── TodoApp.Api/             # Controllers, Program.cs, Swagger, JWT setup
│   ├── TodoApp.Services/        # Business logic (Auth, Task, Category, Token)
│   ├── TodoApp.Core/            # Entities, DTOs, Interfaces (shared contracts)
│   ├── TodoApp.DataAccess/      # DbContext, Repositories, DbSeeder
│   ├── TodoApp.Tests/           # Unit tests
│   └── Dockerfile
├── frontend/
│   ├── src/app/
│   │   ├── core/                # Models, services, guards, interceptors
│   │   ├── features/            # auth, tasks, categories, layout
│   │   └── app.{component,config,routes}.ts
│   ├── Dockerfile + nginx.conf
│   └── tailwind.config.js
└── docker-compose.yml
```

## Quick Start (Docker — recommended)

**Requires:** Docker Desktop running with the Linux engine enabled.

```bash
docker compose up --build
```

After the build completes:

| Service  | URL |
|---|---|
| **Frontend** | http://localhost:4200 |
| **API** | http://localhost:5000 |
| **Swagger** | http://localhost:5000/swagger |

> **Note:** The database runs inside Docker only and is not exposed on a host port.
> The schema is auto-created and seeded on first startup via `EnsureCreated()`.

**Demo credentials:**
- Email: `demo@todo.app`
- Password: `demo1234`

Stop everything:
```bash
docker compose down        # keeps DB volume
docker compose down -v     # also deletes DB data
```

## Manual Setup (without Docker)

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL 14+ running locally

### 1. Database
Create a Postgres database named `todoapp` (default user `postgres` / password `postgres`), or update the connection string in [backend/TodoApp.Api/appsettings.json](backend/TodoApp.Api/appsettings.json).

### 2. Backend

```bash
cd backend
dotnet restore
dotnet run --project TodoApp.Api
```

API starts at `http://localhost:5000`, Swagger at `http://localhost:5000/swagger`.
The DB schema is auto-created and demo data seeded on first run.

### 3. Frontend (new terminal)

```bash
cd frontend
npm install
npm start
```

Open http://localhost:4200. The dev server proxies `/api/*` to `http://localhost:5000` (see `proxy.conf.json`).

## Running Tests

```bash
cd backend
dotnet test
```

Test coverage:
- **AuthServiceTests** — register/login flows, password hashing, error cases
- **TaskServiceTests** — CRUD, validation, user isolation, pagination normalization
- **CategoryServiceTests** — CRUD, mapping, error handling
- **TokenServiceTests** — JWT claims, expiry
- **TaskRepositoryTests** — EF Core InMemory: search, filtering, pagination, user isolation

## API Reference

All endpoints except `/api/auth/*` require `Authorization: Bearer {token}` header.

### Auth
| Method | Path | Body |
|---|---|---|
| POST | `/api/auth/register` | `{ email, userName, password }` |
| POST | `/api/auth/login`    | `{ email, password }` |

### Tasks
| Method | Path | Query / Body |
|---|---|---|
| GET    | `/api/tasks` | `?page=1&pageSize=10&search=...&categoryId=1&isCompleted=true&sortBy=createdAt&sortDir=desc` |
| GET    | `/api/tasks/{id}` | — |
| POST   | `/api/tasks` | `{ title, description?, dueDate?, priority, categoryId? }` |
| PUT    | `/api/tasks/{id}` | `{ title, description?, isCompleted, dueDate?, priority, categoryId? }` |
| DELETE | `/api/tasks/{id}` | — |

**sortBy** values: `createdAt` (default) · `dueDate` · `priority` · `title`  
**sortDir** values: `desc` (default) · `asc`

### Categories
| Method | Path | Body |
|---|---|---|
| GET    | `/api/categories` | — |
| GET    | `/api/categories/{id}` | — |
| POST   | `/api/categories` | `{ name, color }` |
| PUT    | `/api/categories/{id}` | `{ name, color }` |
| DELETE | `/api/categories/{id}` | — |

## Features

- [x] User registration and login (JWT, BCrypt)
- [x] CRUD operations for tasks (title, description, due date, priority, completion)
- [x] CRUD operations for categories (name, color)
- [x] Search tasks by title and description
- [x] Filter tasks by category and completion status
- [x] Sort tasks by created date / due date / priority / title (asc or desc)
- [x] Server-side pagination
- [x] Field-level validation messages on all forms
- [x] User data isolation (users only see their own tasks and categories)
- [x] Swagger documentation
- [x] Unit tests (services + repository)
- [x] Docker support

## Architecture Notes

The backend follows a strict 4-layer architecture:

```
TodoApp.Api          →  Controllers, JWT config, Swagger (presentation)
TodoApp.Services     →  Business logic, manual DTO mapping, validation
TodoApp.Core         →  Entities, DTOs, Interfaces (shared contracts, no dependencies)
TodoApp.DataAccess   →  EF Core DbContext, Repositories, DbSeeder (data access)
```

- All cross-layer communication goes through interfaces defined in `TodoApp.Core`.
- Services depend on repository interfaces, not concrete implementations (DI).
- DTO mapping is done via static methods inside each service (no third-party mapper).

## Security Notes

- Change `Jwt:Key` in `appsettings.json` and `docker-compose.yml` before deploying to production. The current key is for development only.
- Logout is client-side (JWT token is removed from localStorage). Tokens expire after 24 hours.
- Passwords are hashed with BCrypt (work factor 11).
