# Trendyol Go Setup

Status: partial — this file is filled in incrementally as each phase in `PHASES.md` lands. Sections not yet implemented are marked accordingly.

## Where credentials come from

Log in to the Trendyol Go partner panel at **partner.tgomarket.com** → **Kullanıcı Bilgileri** → **Entegrasyon Süreci**. That screen shows:

- **Satıcı ID (Cari ID)** — your Supplier ID
- **API Key**
- **API Seçret** (API Secret)

These three values authenticate every backend call to Trendyol Go. Rotate them from that same screen if they're ever exposed (e.g. shared in a screenshot or chat).

You'll also need three values that are **not** shown on that screen and must be confirmed against the official docs at **developers.tgoapps.com** before going live:

- **BaseUrl** — the Trendyol Go API's base domain. Deliberately left unset in this project rather than guessed.
- **AgentName** — the value the docs require for the `x-agentname` header.
- **ExecutorUser** — the value the docs require for the `x-executor-user` header.

## Where credentials go in this project

Credentials are never committed to the repository, never hardcoded, and never exposed to the web or mobile apps — only the backend (`AslSu.TrendyolGo` project) holds them, injected via `TrendyolGoOptions`.

- **Local development**: `dotnet user-secrets` on `AslSu.Api`:
  ```
  cd backend/src/AslSu.Api
  dotnet user-secrets set "TrendyolGo:SupplierId" "..."
  dotnet user-secrets set "TrendyolGo:ApiKey" "..."
  dotnet user-secrets set "TrendyolGo:ApiSecret" "..."
  dotnet user-secrets set "TrendyolGo:BaseUrl" "..."       # from developers.tgoapps.com
  dotnet user-secrets set "TrendyolGo:AgentName" "..."     # from developers.tgoapps.com
  dotnet user-secrets set "TrendyolGo:ExecutorUser" "..."  # from developers.tgoapps.com
  ```
- **Production**: environment variables (`TrendyolGo__SupplierId`, `TrendyolGo__ApiKey`, `TrendyolGo__ApiSecret`, `TrendyolGo__BaseUrl`, `TrendyolGo__AgentName`, `TrendyolGo__ExecutorUser`) or a secret manager (e.g. Azure Key Vault) — same configuration binding either way.
- `appsettings.json` only ever contains non-secret structure — never any of the above.
- The app itself only *requires* these to be set in a **Production** environment (`ASPNETCORE_ENVIRONMENT=Production`) — it boots fine without them in Development/Testing so the rest of the system (products, orders, auth) keeps working before Trendyol Go is configured.

## Testing the connection

The web admin panel's **API Ayarları** screen (`/api-settings`) shows the configured Supplier ID and a masked API Key (read-only — this screen cannot change credentials), plus an **"API Bağlantısını Test Et"** button. It calls `POST /api/trendyol-settings/test-connection` on the backend, which calls Trendyol Go with the configured credentials and headers and reports back one of:

- **Success** — "Trendyol Go API bağlantısı başarılı."
- **401** — "API Key / API Secret / Supplier ID bilgilerini kontrol edin."
- **403** — "User-Agent veya yetkilendirme bilgilerini kontrol edin."
- **429** — "API rate limitine ulaşıldı."
- **5xx** — "Trendyol Go servisinde geçici hata."
- **Not configured** — "Trendyol Go bağlantı bilgileri henüz yapılandırılmamış."

Every test-connection attempt is recorded in the `SyncLogs` table (operation, endpoint, status code, duration, sanitized error message — never the credentials themselves).

The probe currently calls the packages (orders) GET endpoint given in the integration brief, since Trendyol Go doesn't have a dedicated "ping" endpoint. Its exact query parameters aren't confirmed yet (that lands with real order sync in Phase 8) — a 401/403/429/5xx from this call still tells you definitively whether your credentials/headers are being accepted.

## Product sync (push)

The web admin panel's **Ürünler** screen (`/products`) has a **"Trendyol Go'ya Aktar"** button. It calls `POST /api/trendyol-sync/products/push`, which:

1. Finds every product with `TgoSyncStatus` of `NotSynced` or `Failed`.
2. Splits them into batches (`BatchSplitter`, max 1000 items — TODO: verify the real limit against developers.tgoapps.com).
3. Submits each batch to Trendyol Go's product-creation endpoint.
4. Updates each product's `TgoSyncStatus` (`Pending` on successful submission, `Failed` with a message otherwise) and `TgoLastSyncDate`.
5. Records one row per batch in `BatchRequestLogs` (recent history via `GET /api/trendyol-sync/batch-requests`).

**Known gap**: the brief that shaped this integration gave the price-and-inventory and packages-GET endpoints verbatim, but never the exact createProducts endpoint path or request/response schema. Rather than guess a URL, that endpoint is config-gated:

```
dotnet user-secrets set "TrendyolGo:ProductsEndpointPath" "..."   # from developers.tgoapps.com
```

Until this is set, every push attempt returns a clear "Ürün oluşturma endpoint'i henüz yapılandırılmamış" failure and products stay `Failed` — this is expected, not a bug. Once you have the real path (and, if it differs from what `TrendyolProductPayload` assumes, the real request schema) from the docs, set it and product push will start working; the request body shape may also need adjusting to match the docs at that point (see `TrendyolProductPayload`'s code comments).

## Stock / price sync

*(Not yet implemented — lands in Phases 6–7.)*

## Order sync

*(Not yet implemented — lands in Phases 8–10.)*

## Webhook

*(Not yet implemented — lands in Phase 11.)*

## Troubleshooting

- **"TrendyolGo:SupplierId/ApiKey/ApiSecret/BaseUrl are not fully configured" at startup**: this only happens with `ASPNETCORE_ENVIRONMENT=Production`. Set all four via environment variables (or user-secrets in dev) before deploying.
- **Test Connection always returns "Trendyol Go'ya bağlanılamadı"**: `TrendyolGo:BaseUrl` is likely wrong or unset — confirm the exact domain against developers.tgoapps.com.
- **Test Connection returns 403 even with correct Key/Secret**: check `TrendyolGo:AgentName` / `TrendyolGo:ExecutorUser` against the docs — these are required header values Trendyol Go validates independently of the Basic Auth credentials.
