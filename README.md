# StoneHub

Marketplace app connecting marble & granite dealers/wholesalers/manufacturers with buyers
(retail, builders, architects, contractors, shop owners).

Status: early development — backend scaffold in place, mobile app not yet started.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the system design and the reasoning behind it.

## Planned repo layout

This folder currently holds the backend solution (`StoneHub/`, containing `StoneHub.slnx`).
As the project grows, the plan is three independent repos:

- `StoneHub.Api` — .NET backend
- `StoneHub.Mobile` — React Native app
- `StoneHub.Admin` — admin web panel (built later)

## Getting started

Backend prerequisites and run instructions will be filled in once the database and local
dev setup (Docker Compose) exist.

## Deploying the API

Production is a single EC2 host: the API runs as a self-contained linux-x64 build in
`/opt/stonehub`, started by the `stonehub-api` systemd service, behind Caddy (HTTPS) on
`127.0.0.1:5000`. Postgres runs in the `stonehub-postgres` Docker container on the same host.

```bash
cp deploy/deploy.env.example deploy/deploy.env   # once; set host, key path, public URL
deploy/deploy.sh                                 # SKIP_TESTS=1 to skip dotnet test
```

The script deploys only a clean, pushed commit. It tests, publishes, uploads, backs up the
running build, swaps the binaries in place (server-side config is never overwritten),
restarts the service, and smoke-checks the public URL — rolling back automatically if the
new build doesn't come up. Releases and the last 3 backups live in `/opt/stonehub-releases`;
`/opt/stonehub/REVISION` holds the commit that's live.

EF migrations are applied by the API on startup (currently because prod runs with
`ASPNETCORE_ENVIRONMENT=Development`; see `Program.cs`). A rollback restores the previous
binaries but does not revert migrations.
