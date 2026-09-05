# StoneHub — Backend (StoneHub.Api)

> .NET modular-monolith API for a marble & granite marketplace connecting
> dealers / wholesalers / manufacturers (sellers) with buyers (retail, builders,
> architects, contractors, shop owners).

Read **`ARCHITECTURE.md`** first — it records the decisions behind this design.

---

## Applicable rules

Claude must read these before any task. They live in `.claude/rules/`.

### Universal (always apply)
- `.claude/rules/universal/engineering-principles.md`
- `.claude/rules/universal/working-with-claude.md`

### Stack
- `.claude/rules/stacks/dotnet.md`  (.NET / ASP.NET Core / modular monolith / CQRS)

---

## Stack versions (pinned)

- **.NET 10** (`net10.0`), ASP.NET Core Web API, C# 13
- **EF Core** + **PostgreSQL 16** (one DB, one schema per module)
- **MediatR** — CQRS command/query dispatch
- **FluentValidation** — via a MediatR `ValidationBehavior` pipeline
- **Serilog + OpenTelemetry** — structured logging / tracing
- **In-memory cache** (`IMemoryCache`) — Redis descoped for v1
- **Cloudflare R2** (S3-compatible, `AWSSDK.S3`) — media blob storage (presigned direct uploads)
- **Hangfire** — background jobs (Postgres-backed storage)
- JWT access + refresh tokens (Identity module) — **email + password auth only**

> **v1 scope:** chat/Messaging, Redis, SMS/OTP, email OTP, Google OAuth, and push
> notifications are **descoped** — see `../task.md` → "Descoped for v1".

---

## Architecture — modular monolith

One deployable API, split into modules with **hard boundaries** (own domain model, own
schema; communicate via domain events / published interfaces — never direct cross-module
table access).

| Module | Responsibility |
|---|---|
| Identity | Users, sellers, buyers, auth (JWT + refresh), roles |
| Catalog | Products, listings, categories, stock, search |
| Requirement | Buyer requirements, seller quotations/offers |
| Trust | Reviews, ratings, seller verification, fake-listing reports |
| Media | Photo/video upload orchestration (metadata; files in blob storage) |
| Admin | Cross-module moderation views |
| Billing (stub) | Interfaces only — subscriptions/featured listings wired in later |

Each module = **Domain** (entities, VOs, events, repo interfaces) + **Application**
(CQRS handlers, validators, DTOs, service interfaces) + **Infrastructure** (EF Core
`DbContext`, configs, repo/service impls, DI). Dependencies point inward. Shared
primitives (`Entity`, `AggregateRoot`, `ValueObject`, `Result`, `IDomainEvent`,
`ValidationBehavior`) live in `SharedKernel`.

---

## Project structure

```
StoneHub/
├── ARCHITECTURE.md
├── docker-compose.yml                 # postgres:16 (stonehub)
└── StoneHub/
    ├── StoneHub.slnx
    └── src/
        ├── Api/StoneHub.Api/           # controllers, Program.cs, appsettings
        ├── BuildingBlocks/SharedKernel/
        └── Modules/
            └── <Module>/
                ├── <Module>.Domain/
                ├── <Module>.Application/
                └── <Module>.Infrastructure/
```

---

## Commands

```bash
# Database (Postgres 16)
docker-compose up -d

# Build / run / test  (from StoneHub/)
dotnet build                                  # warnings are errors
dotnet run   --project src/Api/StoneHub.Api
dotnet test

# EF migrations (per module — never edit a shipped migration; add a new one)
dotnet ef migrations add <Name> --project src/Modules/<Module>/<Module>.Infrastructure
dotnet ef database update       --project src/Modules/<Module>/<Module>.Infrastructure
```

---

## Domain glossary (use these words)

- **Seller** — dealer / wholesaler / manufacturer who lists stock
- **Buyer** — retail, builder, architect, contractor, shop owner
- **Listing / Product** — a slab/material offered by a seller
- **Requirement** — a buyer-posted need; sellers respond with **Quotations**
- **Verification** — seller trust badge (Trust module)
- **Featured listing** — a Billing-gated promotion (stubbed today)

---

## The contract with StoneHub.Mobile

The API **generates the OpenAPI spec**; the mobile app generates its typed client from
it. **The spec is the contract, not shared code.** A DTO / route / status-code change is
a contract change — flag it; don't silently reshape a `v1` response.

---

## Project-specific "never do"

- Do not cross module boundaries — no other module's tables/`DbContext`/entities/repos.
  Use domain events or a published interface.
- Do not throw for expected business failures — return `Result` / `Result<T>`.
- Do not put business logic in controllers or data access in handlers.
- Do not edit a migration that has shipped — add a new one.
- Do not break the OpenAPI contract with the mobile app silently.
- (Full list in `.claude/rules/stacks/dotnet.md` → "Stack-specific never-do".)
