# StoneHub — Architecture

Marble & Granite marketplace connecting dealers/wholesalers/manufacturers (sellers) with
buyers (retail, builders, architects, contractors, shop owners).

This document captures the architectural decisions made before implementation started, so
they don't live only in chat history.

## Style: Modular Monolith

One deployable API, internally split into modules with hard boundaries (own domain model,
own DB tables, communicate via interfaces/domain events — never direct cross-module table
access). This gives code-level separation without the operational cost of microservices
(service discovery, distributed transactions, network-call debugging) at a stage where
there's no proven scale or team size that justifies it.

Migration path: because modules never share tables and only talk through published
contracts/events, any module can later be lifted into its own service (Media is the most
likely candidate if load ever demands it) without rewriting the rest.

## Modules

| Module | Responsibility | Status |
|---|---|---|
| Identity | Users, sellers, buyers, wholesalers, auth (JWT + refresh), roles, profile edit | **built** |
| Catalog | Products, listings, categories (DB taxonomy), stock, search (`pg_trgm`) | **built** |
| Media | Photo/video upload orchestration (metadata only — files live in R2) | **built** |
| Inquiries | Seller→wholesaler wholesale inquiries (status state machine) | **built** |
| Messaging | In-app **poll-based** chat (Buyer↔Seller, Seller↔Wholesaler) | **built** — *reinstated, see Descoped note* |
| Requirement | Buyer-posted requirements, seller quotations/offers | planned |
| Trust | Reviews, ratings, seller verification badge, fake-listing reports | planned |
| Admin | Cross-module moderation views (users, products, reviews, reports, premium listings) | planned |
| Billing (stub) | Interfaces only for now — subscriptions/featured listings wired in later | planned |

Each module = Domain + Application (CQRS via MediatR) + Infrastructure slice.
Cross-module communication happens via domain events or a published application interface
(e.g. Catalog's `IProductLookup`, Identity's `ISellerDirectory`), not direct calls between
modules' internals.

## Tech stack (backend)

- ASP.NET Core Web API (.NET, current scaffold on net10.0)
- EF Core + PostgreSQL — one database, one schema per module, no cross-module joins in code
- Cloudflare R2 (S3-compatible) — media blob storage; direct-to-blob via presigned URLs
- In-memory cache (`IMemoryCache`) — Redis descoped for v1; single-instance only
- Hangfire — background jobs (search re-index, quote expiry); **Postgres-backed storage**
- Serilog + OpenTelemetry — logging/tracing
- API versioned from the first controller (`/api/v1/...`)

## Repo strategy

Separate repos, one shared contract:

- `StoneHub.Api` — the .NET backend (this repo)
- `StoneHub.Mobile` — React Native app (buyers + sellers)
- `StoneHub.Admin` — admin panel, likely a plain React/Next.js web app (built later)

Rationale: different release cadence (API ships continuously, mobile goes through app-store
review), different toolchains, different versioning. See chat log / team discussion for
full reasoning if this ever needs revisiting.

**The contract between them is the OpenAPI spec, not shared code.** The backend generates
it automatically; the mobile app generates its typed API client from it as a build step, so
a drift between what the API serves and what the client expects fails the build, not
production.

## Mobile architecture (React Native)

- TypeScript throughout
- Feature-folder structure mirroring backend modules 1:1 (`auth`, `catalog`, `requirements`,
  `trust`, `seller-dashboard`)
- Server state: TanStack Query (React Query)
- Client state: Zustand
- API client generated from the backend's OpenAPI spec
- Media uploads go direct-to-blob (Cloudflare R2) via presigned URLs from the API —
  never proxied through the backend

## Future features — how they plug in

- **AI marble identification**: separate deployable service (own repo/infra), Catalog calls
  it over an internal HTTP contract. Keeps ML/GPU infra decoupled from the API's deploy
  cycle.
- **Transport cost calculator**: stateless module, reads seller/buyer location + product
  weight, no dependency on other modules' internals.
- **Premium subscriptions / featured listings**: `Billing` module exists as an interface
  today (e.g. Catalog checks "is this listing featured" through an abstraction, not a
  hardcoded value) so a real payment gateway (Razorpay/Stripe) can be wired in later without
  touching Catalog.

## Suggested build order

1. Identity + Catalog (seller lists, buyer browses/searches) — walking skeleton ✅
2. Requirement / Quotation module — *not started*
3. Trust (reviews, verification, reporting) — *not started*
4. Buyer↔seller contact — **shipped as in-app chat** (Messaging module), *not* phone reveal
5. Admin panel — *not started*
6. Billing (real), AI identification, transport calculator — *not started*

> **Actually built ahead of this order:** Media (R2 uploads), a v2 marketplace expansion
> (DB category taxonomy, Wholesaler role, `pg_trgm` search), wholesale **Inquiries**, in-app
> **Messaging/chat**, and profile edit. See `../task.md` for the current, authoritative status.

## Descoped for v1 (2026-08-29)

Removed to cut scope and external dependencies. The module boundaries make these
additive to re-introduce later, not a rewrite.

- ~~**Messaging module + SignalR chat**~~ — **REINSTATED (later).** A `Messaging` module
  was built and shipped, but as **HTTP poll-based chat** (client polls with a `?since=`
  cursor — *still no SignalR/realtime*), not the phone-reveal replacement. Chat, not phone
  reveal, is the buyer↔seller contact mechanism. Phone reveal was never built.
- **Redis** — cache falls back to `IMemoryCache`; Hangfire uses Postgres storage.
- **SMS/OTP, email OTP, Google OAuth** — auth is **email + password only**.
- **Push notifications** — notifications are in-app, pull-based.

## Open decisions

- _(none currently — hosting resolved below)_

_Resolved: React Native workflow → **Expo (managed / CNG)**; buyer↔seller contact →
**in-app chat** (Messaging module; supersedes the earlier phone-reveal decision, which was
not built); password reset → **admin-assisted** (no email/OTP); media storage →
**Cloudflare R2** (S3-compatible); hosting → **AWS EC2** (t3.small, Docker + Caddy behind
HTTPS)._
