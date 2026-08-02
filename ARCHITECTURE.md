# StoneUp — Architecture

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
contracts/events, any module can later be lifted into its own service (Media and Messaging
are the most likely candidates if load ever demands it) without rewriting the rest.

## Modules

| Module | Responsibility |
|---|---|
| Identity | Users, sellers, buyers, auth (JWT + refresh), roles |
| Catalog | Products, listings, categories, stock availability, search |
| Requirement | Buyer-posted requirements, seller quotations/offers |
| Trust | Reviews, ratings, seller verification badge, fake-listing reports |
| Messaging | Buyer↔seller chat, notifications (SignalR) |
| Media | Photo/video upload orchestration (metadata only — files live in blob storage) |
| Admin | Cross-module moderation views (users, products, reviews, reports, premium listings) |
| Billing (stub) | Interfaces only for now — subscriptions/featured listings wired in later |

Each module = Domain + Application (CQRS via MediatR) + Infrastructure slice.
Cross-module communication happens via domain events, not direct calls between modules'
internals.

## Tech stack (backend)

- ASP.NET Core Web API (.NET, current scaffold on net10.0)
- EF Core + PostgreSQL — one database, one schema per module, no cross-module joins in code
- Redis — caching + SignalR backplane
- SignalR — real-time chat and quote notifications
- Hangfire — background jobs (search re-index, requirement-match digests, quote expiry)
- Serilog + OpenTelemetry — logging/tracing
- API versioned from the first controller (`/api/v1/...`)

## Repo strategy

Separate repos, one shared contract:

- `StoneUp.Api` — the .NET backend (this repo)
- `StoneUp.Mobile` — React Native app (buyers + sellers)
- `StoneUp.Admin` — admin panel, likely a plain React/Next.js web app (built later)

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
  `trust`, `chat`, `seller-dashboard`)
- Server state: TanStack Query (React Query)
- Client state: Zustand
- API client generated from the backend's OpenAPI spec
- Media uploads go direct-to-blob-storage via presigned URLs from the API — never proxied
  through the backend

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

1. Identity + Catalog (seller lists, buyer browses/searches) — walking skeleton
2. Requirement / Quotation module
3. Trust (reviews, verification, reporting)
4. Messaging / chat
5. Admin panel
6. Billing (real), AI identification, transport calculator

## Open decisions

- React Native: Expo vs. bare workflow — not yet decided
- Hosting target (Azure vs AWS) — not yet decided
