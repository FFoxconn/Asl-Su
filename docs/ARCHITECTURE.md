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
│   ├── AslSu.Infrastructure/  # EF Core DbContext, migrations, JWT auth, service implementations, BackgroundServices/ — depends on Domain + Application
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
- `Products/`, `Inventory/`, `SellUnsell/`, `Orders/`, `PackageStatus/`, `BatchRequests/`, `Webhook/`, `Connection/` — one client interface + implementation per domain
- `Configuration/` — `TrendyolGoOptions` (SupplierId, ApiKey, ApiSecret, BaseUrl, plus the various config-gated endpoint paths/webhook secret added across later phases), bound from configuration
- `Http/` — typed `HttpClient` registration via `IHttpClientFactory`, with a delegating handler for auth headers and retry/backoff on 429/5xx
- `Exceptions/` — `TrendyolApiException`/`TrendyolAuthException`/`TrendyolRateLimitException`, mapped to consistent Turkish user-facing messages via `TrendyolErrorMessages`

Only this project ever holds or transmits Trendyol Go credentials.

## Application/Infrastructure feature folders

Beyond the original Auth/Catalog/Products/StockPrice slices, later phases added:

- `ProductSync/`, `StockPriceSync/`, `SaleStatusSync/`, `BatchPolling/` — the delta-only push services behind the Products page's Trendyol Go buttons
- `OrderSync/` — pulls and upserts orders by `PackageId`
- `OrderWorkflow/` — the four-stage Yeni → Kabul Edildi → Hazırlanıyor → Hazırlandı → Teslim Edildi transition service
- `Couriers/` — a purely local (non-Trendyol) courier entity/service, plus order-to-courier assignment on `IOrderService`
- `Webhooks/` — `IWebhookService`, the validate → dedupe → record → trigger-pull pipeline behind `WebhooksController` (the one `[AllowAnonymous]` controller, since Trendyol Go doesn't carry our JWT)
- `Infrastructure/BackgroundServices/` — two singleton `BackgroundService`s that are inert until Trendyol Go is configured: `OrderPollingBackgroundService` (a webhook-delivery fallback, ~45s cadence) and `ProductAutoSyncBackgroundService` (automates the manual product/stock/price/sale-status push buttons, ~1min cadence). Both are rate-limit aware, backing off to a longer interval when a cycle reports Trendyol Go's rate limit was hit.

## Secrets & logging

See `TRENDYOL_GO_SETUP.md` for exactly where credentials go. In short: never in `appsettings.json`, never in source, never sent to web/mobile, never logged. Log level configuration lives in code (`Program.cs`'s `UseSerilog(...)` call) rather than appsettings, with `System.Net.Http.HttpClient` explicitly pinned to `Warning` so `HttpClientFactory`'s built-in request logging can never surface the Trendyol Go Basic Auth header.
