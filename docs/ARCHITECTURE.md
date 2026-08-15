# Architecture

## Overview

Asl-Su is a monorepo with one backend serving two clients:

```
React Web (web/)         ─┐
                           ├─▶  ASP.NET Core Web API (backend/)  ─▶  SQL Server
React Native Mobile (mobile/) ─┘                │
                                                 └─▶  Trendyol Go Market API
```

- **web/** and **mobile/** never call Trendyol Go directly and never hold Trendyol Go credentials. They authenticate to the backend with JWT and call the backend's REST API only.
- **backend/** owns all business logic, persistence, and the Trendyol Go integration.

## Backend layout

```
backend/
├── src/
│   ├── AslSu.Domain/          # Entities & enums, no external dependencies
│   ├── AslSu.Application/     # Services/use-cases, DTOs, interfaces — depends on Domain
│   ├── AslSu.Infrastructure/  # EF Core DbContext, migrations, JWT auth, background jobs — depends on Domain + Application
│   ├── AslSu.TrendyolGo/      # Isolated Trendyol Go API client library — no dependency on Domain/Infrastructure
│   └── AslSu.Api/             # ASP.NET Core Web API host — controllers, Program.cs — depends on all of the above
└── tests/
    ├── AslSu.Domain.Tests/
    ├── AslSu.Application.Tests/
    ├── AslSu.TrendyolGo.Tests/       # mocked HttpClient: auth headers, batching, retry/backoff, response mapping
    └── AslSu.Api.IntegrationTests/   # WebApplicationFactory-based endpoint tests
```

This is a pragmatic 4-layer split (Domain/Application/Infrastructure/Api) plus an isolated `AslSu.TrendyolGo` client library — enough to keep Trendyol-specific code testable, swappable, and to keep credentials contained to one place, without full Clean-Architecture/CQRS ceremony (no mediator, no repository-per-entity by default).

## Trendyol Go integration module

`AslSu.TrendyolGo` is organized by domain, mirroring how the Trendyol Go API itself is structured:

- `Authentication/` — Basic Auth header + required-headers builder
- `Products/`, `Inventory/`, `Orders/`, `PackageStatus/`, `BatchRequests/`, `Webhook/` — one client interface + implementation per domain
- `Configuration/` — `TrendyolGoOptions` (SupplierId, ApiKey, ApiSecret, BaseUrl), bound from configuration
- `Http/` — typed `HttpClient` registration via `IHttpClientFactory`, with a delegating handler for auth headers and Polly-based retry/backoff on 429/5xx

Only this project ever holds or transmits Trendyol Go credentials.

## Secrets

See `TRENDYOL_GO_SETUP.md` for exactly where credentials go. In short: never in `appsettings.json`, never in source, never sent to web/mobile, never logged.
