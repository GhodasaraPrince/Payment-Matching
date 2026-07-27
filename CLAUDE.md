# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Upload a System CSV and a Provider CSV, match them by `orderId + currency`, and resolve mismatches
by accepting either the System or Provider side. Each row is classified as `Matched`,
`AmountMismatch`, `OnlySystem`, or `OnlyProvider`.

- **Frontend:** Angular 20 (standalone components, signals)
- **Backend:** .NET 10 Web API, Domain / Application / Infrastructure / Api layering
- **Database:** PostgreSQL via EF Core + Npgsql

## Commands

### Run everything (Docker)

```bash
docker compose up --build
```

Frontend: http://localhost:4200 · API/Swagger: http://localhost:5082/swagger · Postgres: localhost:5432
(`postgres`/`postgres`, db `payments_matching`). EF Core migrations apply automatically on API
startup. nginx proxies `/api` to the API container in this mode, so no CORS config is needed.

### Backend (without Docker)

```bash
cd backend
export ConnectionStrings__Default="Host=localhost;Database=payments_matching;Username=postgres;Password=postgres"
dotnet run --project src/PaymentsMatchingTool.Api
```

Run all tests:

```bash
cd backend
dotnet test
```

Run a single test (standard `dotnet test` filter syntax), e.g.:

```bash
dotnet test --filter "FullyQualifiedName~PaymentMatchingServiceTests"
```

Add a new EF Core migration (from `backend/`):

```bash
dotnet ef migrations add <Name> --project src/PaymentsMatchingTool.Infrastructure --startup-project src/PaymentsMatchingTool.Api
```

### Frontend (without Docker)

```bash
cd frontend/payments-matching-tool
npm install
npm start   # ng serve, http://localhost:4200
npm test    # ng test / Karma
```

`src/environments/environment.development.ts` points `ng serve` at `http://localhost:5082/api`.

## Architecture

### Backend layering (`backend/src`)

Standard Clean Architecture split, dependencies point inward:

- **`PaymentsMatchingTool.Domain`** — `MatchBatch`, `PaymentMatch` entities, `MatchStatus` /
  `ResolutionSide` enums. No dependencies on other layers.
- **`PaymentsMatchingTool.Application`** — business logic, no EF Core/infra dependency.
  - `Csv/` — `ICsvPaymentParser` parses `orderId,amount,currency` rows; throws
    `CsvValidationException` for malformed input or unsupported currencies.
  - `Matching/PaymentMatchingService` — pure in-memory matching algorithm: builds a
    dictionary per file keyed by `(orderId, currency)` (last row wins on duplicates within a
    file), unions the keys, and classifies each into a `MatchStatus`. Amounts compared via
    `Math.Round(x, 2)`.
- **`PaymentsMatchingTool.Infrastructure`** — `AppDbContext` (EF Core/Npgsql), entity
  `Configurations/`, migrations, and `Services/MatchRunService` which orchestrates
  parse → match → persist-as-a-new-batch, plus batch/item queries and the resolve mutation.
- **`PaymentsMatchingTool.Api`** — `Program.cs` wires DI, CORS (`Cors:AllowedOrigins` config),
  Swagger, and auto-applies migrations (`db.Database.Migrate()`) on startup. Single
  `MatchesController` exposes the endpoints below via `Contracts/*Dto` request/response types.

Each match run always creates a **new** `MatchBatch` rather than overwriting the previous one; old
batches stay queryable by id and are never deleted.

### API

- `POST /api/matches/run` — multipart (`SystemFile`, `ProviderFile`) → `{ batchId, summary, items[] }`
- `GET /api/matches/{batchId}?filter=unresolved|resolved|all` — list items for a batch
- `PATCH /api/matches/items/{id}/resolve` — body `{ "resolutionSide": "System" | "Provider" }`

### Frontend (`frontend/payments-matching-tool/src/app`)

Single feature module under `payments-matching/`: `payment-matching.service.ts` (HTTP calls to the
API), `models.ts` (DTOs mirroring the backend contracts), and the `payments-matching.ts`/`.html`/
`.scss` standalone component driving upload, results table, filtering, and resolve actions. No
routing module — `app.ts`/`app.html` just hosts this one component.

## Key domain rules (don't rederive from scratch — see README "Assumptions" for full list)

- Currency is restricted to `USD, EUR, INR, GBP`; anything else is a CSV validation error.
- Duplicate `orderId + currency` within one file: last row read wins.
- No authentication — single-user internal tool by design.
- Resolved/Resolution Side columns and Accept buttons show for every row, not just mismatches.
