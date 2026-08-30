# .NET / ASP.NET Core Stack Rules

Apply when working in a .NET backend (C# 12+/.NET 8+; StoneUp targets **net10.0**).
Assumes an **ASP.NET Core Web API** organized as a **modular monolith** with CQRS
(MediatR), EF Core, and a DDD-flavoured domain layer.

This file is a **delta** over `universal/engineering-principles.md` and
`universal/working-with-claude.md`. It does not repeat the universal rules (type
safety, file length, no magic values, errors-are-first-class, validate-at-boundaries).
Those still apply, expressed in C# idioms below.

---

## Language baseline

- **Nullable reference types on** (`<Nullable>enable</Nullable>`). No `#nullable
  disable`. A `?` on a type is a deliberate statement that null is valid; the absence
  of `?` is a guarantee it isn't. Don't defeat it with `!` (null-forgiving) except
  where the compiler genuinely can't see a checked invariant — and prefer restructuring
  over `!`.
- **No `dynamic`.** No `object` as a stand-in for a real type outside genuine
  serialization boundaries.
- **File-scoped namespaces**, one top-level type per file (`namespace Catalog.Domain;`).
- **`sealed` by default** for classes not designed for inheritance — every handler,
  validator, DTO, and service in this codebase is `sealed`. Match that.
- **Records for immutable data** — commands, queries, DTOs, value objects, events are
  `record` / `readonly record struct`. Reserve `class` for entities with identity and
  behaviour, and for services.
- **Primary constructors for dependency injection** — handlers/services take their
  dependencies via primary constructor params, exactly as the existing handlers do:
  ```csharp
  public sealed class LoginCommandHandler(
      IIdentityService identityService,
      IJwtTokenService jwtTokenService)
      : IRequestHandler<LoginCommand, Result<AuthResultDto>>
  ```
- **`var` when the type is obvious from the right-hand side**, explicit type otherwise.
- Prefer expression-bodied members for one-liners; block bodies when it aids readability.
- Enable warnings-as-errors in CI; treat analyzer warnings as defects, not noise.

---

## Modular monolith — module boundaries are sacred

This is the highest-leverage rule in the codebase. One deployable API, split into
modules (**Identity, Catalog, Requirement, Trust, Media, Admin, Billing**)
with **hard boundaries**.

- **A module never touches another module's tables, `DbContext`, entities, or
  repositories.** No cross-module joins in SQL or LINQ. Each module owns its schema.
- **Cross-module communication happens through published contracts, not internals:**
  - **Domain events** (`IDomainEvent`) for "this happened" notifications another module
    reacts to (e.g. `SellerRegisteredEvent` → Catalog provisions a seller space).
  - A **thin public application interface** exposed by the owning module and consumed
    via DI, when a module needs to *ask* another module something. The contract lives
    in the owning module's `Application` project; the consumer depends on the abstraction,
    never the implementation.
- A module's `Domain` and `Infrastructure` internals are **private to that module**.
  If you're referencing `Catalog.Infrastructure` from `Identity.*`, stop — that's a
  boundary violation. Route it through an event or a published interface instead.
- **Billing is interfaces-only today.** Consume it through its abstraction (e.g.
  "is this listing featured?") so a real gateway wires in later without touching callers.

The payoff — any module can later be extracted into its own service — only holds if
these boundaries stay clean. Every shortcut across them is a future rewrite.

---

## Module internal structure — Domain / Application / Infrastructure

Each module is three projects with a strict dependency direction:

```
<Module>.Domain          → entities, value objects, domain events, repository
                           INTERFACES. No EF, no MediatR, no framework deps.
<Module>.Application      → CQRS commands/queries + handlers + validators, DTOs,
                           service interfaces. Depends on Domain + SharedKernel + MediatR.
<Module>.Infrastructure   → EF Core DbContext, entity configs, repository & service
                           IMPLEMENTATIONS, module DI registration. Depends on Application.
```

Dependencies point **inward**: Infrastructure → Application → Domain. Domain depends on
nothing but `SharedKernel`. If Domain references EF Core or MediatR, the layering is
broken.

`SharedKernel` (BuildingBlocks) holds the cross-module primitives only: `Entity`,
`AggregateRoot`, `ValueObject`, `IDomainEvent`, `Result`, `ValidationBehavior`. Nothing
domain-specific goes here.

---

## CQRS with MediatR — one operation per file

Every write is a **Command**, every read is a **Query**, each with its own handler.

- **One command/query + its handler + its validator per feature folder**, named after
  the operation in the domain's language:
  ```
  Products/
    CreateProductListingCommand.cs
    CreateProductListingCommandHandler.cs
    CreateProductListingCommandValidator.cs
  ```
- **Commands and queries are `record`s.** Handlers are `sealed`, implement
  `IRequestHandler<TRequest, Result<TResponse>>`, and take dependencies via primary
  constructor.
- **Handlers return `Result` / `Result<T>`** — never throw for expected business
  failures (see the Result section). The handler orchestrates; it does not contain
  data-access code (that's the repository) or HTTP concerns (that's the controller).
- **Commands mutate through the domain, not by setting properties.** A handler loads an
  aggregate, calls a behaviour method on it, and persists it — it does not reach into
  the entity's state and assign fields.
- **Queries bypass the domain for reads.** Read-side handlers may project straight to
  DTOs via a query service (`IProductQueryService`) using `AsNoTracking`, rather than
  rehydrating full aggregates. Don't force reads through the write model.
- Keep handlers thin. A handler over ~40 lines is doing something a domain method or a
  service should own.

---

## Validation — FluentValidation at the pipeline boundary

- Every command/query that needs validation has a `sealed` `AbstractValidator<T>`
  living next to it. Input shape/format rules go here:
  ```csharp
  public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
  {
      public LoginCommandValidator()
      {
          RuleFor(x => x.Email).NotEmpty().EmailAddress();
          RuleFor(x => x.Password).NotEmpty();
      }
  }
  ```
- Validation runs automatically via the MediatR `ValidationBehavior` pipeline — do
  **not** call validators by hand inside handlers, and do not scatter `if (string.
  IsNullOrEmpty(...))` checks through handler bodies.
- **Input validation ≠ business-rule validation.** "Email is well-formed" is a
  validator's job; "email is already registered" is a domain/handler decision that
  returns a `Result.Failure`. Keep the two in their proper places.
- Validation failures surface as structured, field-level errors to the client — never
  a generic 500.

---

## The Result pattern — expected failures are values, not exceptions

`SharedKernel.Result` / `Result<T>` model expected outcomes. Follow it consistently.

- **Handlers and domain operations return `Result` for anything a caller is expected to
  handle** — not found, unauthorized, conflict, business-rule rejection. Propagate with
  the early-return style already in the codebase:
  ```csharp
  var credentialsResult = await identityService.ValidateCredentialsAsync(email, password, ct);
  if (credentialsResult.IsFailure)
      return Result.Failure<AuthResultDto>(credentialsResult.Error!);
  ```
- **Exceptions are for the exceptional** — programmer errors, infrastructure faults
  (DB down, broker unreachable), truly-can't-continue states. Don't use `throw` for
  ordinary "the user did something invalid" flows.
- **Never swallow.** No empty `catch`. A global exception-handling middleware maps
  unhandled exceptions to `ProblemDetails` responses; individual handlers don't each
  wrap themselves in try/catch (mirrors universal rule 20 — one top-level boundary).
- When you need richer error info than a string, extend the error model deliberately
  (an error code enum / typed error) rather than string-matching `Error` at call sites.

---

## Domain layer — rich models, not anemic bags of setters

- Entities inherit `Entity` / `AggregateRoot`; identity comes from the base type.
- **Encapsulate state.** Properties have `private set` (or `init`); mutation happens
  through intention-revealing methods (`product.UpdateStock(qty)`, not
  `product.Stock = qty`). Invariants are enforced inside those methods.
- **Construct through factory methods** that return `Result<TEntity>` when creation can
  fail its invariants, rather than a public constructor that can build an invalid entity.
- **Value objects** (`Money`, `Location`) are immutable, equality-by-value (`ValueObject`
  / `readonly record struct`), and validate themselves on construction. Don't pass raw
  `decimal` + `string currency` around when `Money` exists.
- **Aggregates raise domain events** (`AddDomainEvent`) for cross-module facts;
  dispatch happens on save, not inline in the middle of a behaviour method.
- Repository **interfaces** live in `Domain` (`IProductRepository`,
  `ISellerProfileRepository`); implementations live in `Infrastructure`.

---

## Persistence — EF Core, one DbContext per module

- **One `DbContext` per module** (`CatalogDbContext`), mapped to that module's own
  **schema**. No shared context, no cross-module `DbSet`s.
- **Configure via `IEntityTypeConfiguration<T>`** classes (`ProductConfiguration`) in
  `Infrastructure/Persistence`, applied with `ApplyConfigurationsFromAssembly`. Don't
  configure entities inline in `OnModelCreating`.
- **Queries that don't track should say so** — `AsNoTracking()` for read/query-service
  paths. Tracking is for the write path where you load-mutate-save an aggregate.
- **No lazy loading.** Load what you need explicitly (`Include`, or projections). Lazy
  loading hides N+1 queries.
- **Repositories return domain entities**, not EF query shapes leaking `IQueryable` to
  callers. Query services may return DTOs directly for reads.
- **Migrations are generated, reviewed, and immutable once merged.** Create with
  `dotnet ef migrations add <Name> --project <Module>.Infrastructure`. Never hand-edit
  a migration that has shipped; never delete/rewrite applied migrations — add a new one.
  Migrations are code — read the generated migration before committing it.
- Parameterization is automatic through EF/LINQ. If you ever drop to raw SQL, use
  parameters — never string interpolation into the query.

---

## Controllers — thin transport, versioned from day one

- Controllers **parse the request → send a MediatR command/query → map the `Result` to
  an `ActionResult`.** That is the whole controller. No business logic, no data access,
  no validation logic (universal rule 8).
  ```csharp
  var result = await mediator.Send(new LoginCommand(dto.Email, dto.Password), ct);
  return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Error);
  ```
- **Version every route from the first controller** — `/api/v1/...`. New breaking shapes
  go to a new version, not a mutated v1.
- Map `Result` failures to the right status codes through a shared helper/middleware, so
  the mapping is defined once, not re-invented per action.
- DTOs in / DTOs out — never accept or return EF entities or domain aggregates over the
  wire. Request/response records live in the module's `Application` layer.
- `CancellationToken` is a parameter on every action and is threaded through to `Send`.

---

## Async & cancellation

- **Async all the way** — no `.Result`, no `.Wait()`, no `.GetAwaiter().GetResult()`.
  Those deadlock and hide exceptions.
- **`CancellationToken` flows through every async signature** — controller → MediatR →
  handler → repository → EF. The existing handlers already take `CancellationToken ct`;
  keep passing it (don't drop it at the repository call).
- Name async methods `...Async`.
- Don't `async void` except for genuine event handlers.
- Offload long-running work (search re-index, digests, quote-expiry sweeps) to
  **Hangfire** jobs — never do it inline in a request handler (universal rule 10).

---

## Logging & observability — Serilog + OpenTelemetry, structured

- Use the injected `ILogger<T>` with **message templates**, never string interpolation
  in the message and never `Console.WriteLine`:
  ```csharp
  logger.LogInformation("Product listed {ProductId} by seller {SellerId}", id, sellerId);
  ```
- Structured properties (the `{Named}` holes), not concatenated strings — that's what
  makes logs queryable in the Serilog/OTel pipeline.
- Log level discipline: `Debug` for dev detail, `Information` for business milestones,
  `Warning` for recoverable oddities, `Error` for failures needing attention.
- **Never log secrets, JWTs, passwords, full PII, or raw request bodies.**
- Propagate trace context (OpenTelemetry) across module boundaries and background jobs.

---

## Security

- **JWT access + refresh tokens** (Identity module owns issuance via `IJwtTokenService`).
  Access tokens short-lived; refresh tokens rotated and revocable.
- **Authorize every protected endpoint** — role/policy checks, and never trust a
  client-supplied user/seller id; derive identity from the validated token claims.
- **Secrets never live in `appsettings.json`.** Use user-secrets in dev, a secret
  manager / environment in prod. `appsettings.json` holds non-secret config only.
- Hash passwords with a modern KDF (ASP.NET Core Identity's hasher / BCrypt / Argon2) —
  never store or log plaintext.
- Enforce authorization at the application boundary, and re-check ownership inside
  handlers for resource-scoped operations (a seller can only edit their own listings).
- Rate-limit public endpoints (auth especially).

---

## The mobile contract is the OpenAPI spec

- The backend **generates the OpenAPI spec**; `StoneUp.Mobile` generates its typed API
  client from it. **The spec is the contract, not shared code.**
- A change to a request/response DTO, a route, or a status code is a **contract change**.
  Treat it as one: it will break the client build if the client isn't regenerated. Call
  it out explicitly; don't quietly reshape a v1 response.
- Keep DTOs and their annotations accurate so the generated spec matches reality — a
  wrong spec produces a wrong client that fails at runtime, not build time.

---

## Testing

- **xUnit** for tests, one test project per module where it earns its keep.
- **Domain + handler logic** is the priority (universal rule 12): business rules,
  auth/authorization decisions, `Result` failure paths, value-object invariants, money
  math. Mock collaborators through their interfaces (NSubstitute / Moq).
- **Integration tests** for the EF/persistence slice use a **real PostgreSQL via
  Testcontainers**, not an in-memory provider (the in-memory provider hides real query
  and constraint behaviour).
- Test module boundaries: a handler test should never need to spin up another module.
  If it does, the boundary is leaking.
- Run `dotnet build` (warnings-as-errors) + `dotnet test` before declaring done.

---

## Common pitfalls

- **Reaching across module boundaries** (referencing another module's `DbContext` /
  entities / repository) instead of using an event or published interface.
- **Anemic domain** — public setters everywhere, all logic in handlers. Push invariants
  into the entity.
- **Throwing for expected failures** instead of returning `Result` — and conversely,
  returning `Result` for truly-exceptional infrastructure faults.
- **Calling validators manually** inside handlers instead of relying on the pipeline.
- **`.Result` / `.Wait()`** sync-over-async deadlocks.
- **Dropping `CancellationToken`** somewhere in the chain so cancellation stops working.
- **Business logic in controllers**; **data access in handlers** instead of repositories.
- **Editing a shipped migration** instead of adding a new one.
- **In-memory EF provider for integration tests** giving false confidence.
- **Secrets in `appsettings.json`**; **logging tokens/PII**.
- **String-interpolated log messages** that aren't queryable.
- **Leaking EF entities / aggregates over the API** instead of DTOs.

---

## Stack-specific never-do

- Do not access another module's tables, `DbContext`, entities, or repositories
- Do not reference one module's `Infrastructure`/`Domain` internals from another module
- Do not throw exceptions for expected business failures — return `Result`
- Do not swallow exceptions or use empty `catch` blocks
- Do not put business logic in controllers or data access in handlers
- Do not call FluentValidation validators by hand — use the MediatR pipeline
- Do not expose EF entities or domain aggregates over the API — use DTOs
- Do not use `.Result` / `.Wait()` / `.GetAwaiter().GetResult()`
- Do not drop `CancellationToken` from an async call chain
- Do not use `dynamic`, disable nullable, or paper over nulls with `!`
- Do not edit or delete a migration that has been merged — add a new one
- Do not put secrets in `appsettings.json` or log tokens/passwords/PII
- Do not use `Console.WriteLine` — use structured `ILogger<T>`
- Do not break the OpenAPI contract silently — a DTO/route/status change is a contract change
- Do not introduce a new top-level route without versioning it (`/api/v1/...`)
