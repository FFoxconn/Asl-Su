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

## Stock / price sync (push)

The Products page has a **"Stok/Fiyat Gönder"** button. It calls `POST /api/trendyol-sync/stock-price/push`, which:

1. Finds only `StoreProductInventory` rows that have never been synced, or whose `Quantity`/`SalePrice` differ from the last-synced values — a true delta push, not a full resend every time.
2. Splits them into batches (same `BatchSplitter` as product sync) and submits each to the **real** price-and-inventory endpoint (`POST /integrator/product/grocery/suppliers/{sellerId}/products/price-and-inventory`, given verbatim in the integration brief — no config-gating needed here, unlike product creation).
3. On a successful batch, advances that row's last-synced markers so it won't be resent until it changes again. On failure, the row is left as "changed" so it's retried on the next push.
4. Records one row per batch in `BatchRequestLogs`, same as product sync.

`Store.TgoStoreId` exists in the schema as of this phase for mapping a branch to Trendyol Go's own store/warehouse identifier, if the payload needs one. There's no update endpoint or UI for it yet — only `POST /api/stores` (create) exists — so for now it has to be set directly against the database if/when it's needed.

## Batch result polling

The Products page has a **"Parti Sonuçlarını Kontrol Et"** button. It calls `POST /api/trendyol-sync/batch-requests/poll`, which checks every `BatchRequestLog` row still `Pending` against Trendyol Go's batch-result endpoint and updates its status/success count/failure count/failure reasons.

**Known gap**: like createProducts, the exact `getBatchRequestResult` endpoint path was never given in the integration brief. It's config-gated the same way:

```
dotnet user-secrets set "TrendyolGo:BatchResultEndpointPath" "..."   # from developers.tgoapps.com, with a {batchRequestId} placeholder, e.g. "/integrator/.../batch-requests/{batchRequestId}"
```

Until this is set, polling reports every pending batch as "still processing" rather than a hard failure (since a batch may genuinely still be processing on Trendyol's side — we just can't tell yet). Once set, the response's assumed `{status, successCount, failureCount, failureReasons}` shape may also need adjusting to match the real docs (see `TrendyolBatchResultClient`'s code comments) — it currently falls back to an "Unknown" status rather than crashing if the shape doesn't match.

## Sell / unsell (satışa aç / satıştan kaldır)

Each product's stock/price row (in the "Stok/Fiyat" editor on the Products page) shows its current sale status and a toggle button — **"Satıştan Kaldır"** (with a reason-code text field) or **"Satışa Aç"**. This only stages the change locally.

The Products page's **"Satış Durumu Gönder"** button calls `POST /api/trendyol-sync/sale-status/push`, which:

1. Finds only rows whose `IsOnSale` differs from the last-synced value — delta-only, same pattern as stock/price sync.
2. Splits into batches and submits each to the sell/unsell endpoint.
3. On success, advances `LastSyncedIsOnSale`/`SaleStatusLastSyncedAt`; on failure, leaves it so the row is retried next time.
4. Records one row per batch in `BatchRequestLogs` (`OperationType = SellUnsell`).

**Known gap, same shape as product creation and batch-result polling**: neither the sell/unsell endpoint path nor Trendyol Go's official set of unsell reason codes were given in the integration brief.

```
dotnet user-secrets set "TrendyolGo:SellUnsellEndpointPath" "..."   # from developers.tgoapps.com
```

Until this is set, every push attempt returns "Satışa açma/kapatma endpoint'i henüz yapılandırılmamış" and the batch is recorded as `Failed`. The reason-code field in both the API (`SetSaleStatusRequest.ReasonCode`) and the web UI is deliberately **free text**, not a closed dropdown — using a value the API doesn't recognize will surface as a 400-class failure through the normal error handling rather than crashing, but you should confirm the real supported codes against developers.tgoapps.com before relying on this for a real unsell.

## Order sync (pull)

The web admin panel's **Siparişler** screen (`/orders`) has a **"Siparişleri Çek"** button. It calls `POST /api/trendyol-sync/orders/pull`, which:

1. Calls the packages GET endpoint (same one the connection test probes — path given verbatim in the integration brief, query parameters not sent yet).
2. Parses the response into packages, best-effort — the root may be a bare array or wrapped under a common paging key (`content`/`packages`/`orders`/`items`/`data`); each package's original JSON is always kept in full regardless of how well the named fields parsed.
3. Upserts each package by `Order.PackageId`: inserts a new `Order`/`OrderItem`s (and a `Customer` row, if the payload has a customer name) if the package hasn't been seen before, or updates the existing row and replaces its items if it has — **re-pulling never creates a duplicate order or duplicate line items**.
4. Maps each line's `barcode` to a local `Product` if one exists with that barcode (`OrderItem.ProductId` stays `null` otherwise).
5. Always stores the original response for that package in `Order.RawPayloadJson`, so nothing is lost even if the field mapping below turns out to be wrong.

**Known gap, same shape as Phases 5–7**: the packages GET endpoint's exact response schema was never given in the integration brief — only its path was. Field names in `TrendyolPackageDto` (id/packageId, orderNumber, status, orderDate, customerFirstName/customerLastName, totalPrice, lines[].barcode, etc.) are a best-effort guess based on common marketplace API conventions, not confirmed against developers.tgoapps.com. Likewise, no store/warehouse field was confirmed in the payload — a package is matched to a `Store` via `Store.TgoStoreId` if the parsed value matches one, otherwise it falls back to the first store in the system. If you have more than one store, confirm the real field and adjust `TrendyolOrderClient`'s parsing before relying on this for multi-store order routing.

`Order.WorkflowStatus` (the local Yeni → Kabul Edildi → Hazırlanıyor → Hazırlandı → Teslim Edildi fulfilment flow) is set to `New` only when an order is first inserted — pulling again never resets it. Advancing it through the workflow is described next.

## Order status workflow

Each order in the **Siparişler** screen shows exactly one button — the single valid next step for its current `WorkflowStatus`:

| Current step | Button | Calls | Next step |
| --- | --- | --- | --- |
| Yeni | Kabul Et | `POST /api/orders/{id}/accept` | Kabul Edildi |
| Kabul Edildi | Hazırlanmaya Başla | `POST /api/orders/{id}/start-preparing` | Hazırlanıyor |
| Hazırlanıyor | Hazırlandı Olarak İşaretle | `POST /api/orders/{id}/mark-prepared` | Hazırlandı |
| Hazırlandı | Teslim Et | `POST /api/orders/{id}/deliver` | Teslim Edildi |

Calling a step out of order (e.g. "Hazırlanmaya Başla" before "Kabul Et"), on a missing order, or twice in a row returns a 409/404 instead of silently succeeding — the workflow can only move forward one step at a time.

Kabul Et / Hazırlandı Olarak İşaretle / Teslim Et also try to notify Trendyol Go (`accept`/`invoice`/`ship` respectively) through `ITrendyolPackageStatusClient`. **The local step always advances regardless of whether that notification succeeds** — this is deliberate: the panel's own fulfilment workflow shouldn't be blocked by Trendyol Go being unreachable or (right now) unconfigured. Each response includes `trendyolNotified` and `trendyolMessage` so you can see whether Trendyol Go was actually informed; the web UI surfaces this as an inline success/warning message under the row. "Hazırlanıyor" (Hazırlanmaya Başla) is a purely local milestone — no Trendyol Go call was given for it in the integration brief.

**Known gap, same shape as product creation, batch-result polling, and sell/unsell**: none of the accept/invoice/ship endpoint paths were given in the integration brief.

```
dotnet user-secrets set "TrendyolGo:AcceptOrderEndpointPath" "..."   # from developers.tgoapps.com, with a {packageId} placeholder
dotnet user-secrets set "TrendyolGo:InvoiceOrderEndpointPath" "..."  # same shape
dotnet user-secrets set "TrendyolGo:ShipOrderEndpointPath" "..."     # same shape
```

Until these are set, `trendyolNotified` stays `false` and `trendyolMessage` explains why — but the order still moves through the local workflow normally.

## Alternative product / substitution

When a line item's barcode doesn't match any local product (shown as "Eşleşmedi" in the Orders screen), or a picker needs to swap in a different product entirely, the item detail table has an **"İkame Ürün"** picker. Selecting a product and confirming calls `PUT /api/orders/{orderId}/items/{itemId}/substitute`, which sets the item's `ProductId`/`Barcode` to the chosen product's, flags `IsSubstitution = true`, and remembers the original barcode in `SubstitutedForBarcode`.

**Known gap**: the integration brief never gave a Trendyol Go endpoint for reporting a substitution back to Trendyol — this action is **local only**. If Trendyol Go has a real substitution-reporting call, it isn't wired up yet; confirm against developers.tgoapps.com before assuming Trendyol is aware of any substitution made here.

## Courier management

Kuryeler (couriers) and per-order courier assignment are **not** part of the Trendyol Go integration — they're a local operations feature (who delivers this order), added alongside Phase 10. `GET`/`POST /api/couriers` manage the courier list; `PUT /api/orders/{id}/courier` assigns one to an order. No Trendyol Go calls are involved.

## Webhook

*(Not yet implemented — lands in Phase 11.)*

## Troubleshooting

- **"TrendyolGo:SupplierId/ApiKey/ApiSecret/BaseUrl are not fully configured" at startup**: this only happens with `ASPNETCORE_ENVIRONMENT=Production`. Set all four via environment variables (or user-secrets in dev) before deploying.
- **Test Connection always returns "Trendyol Go'ya bağlanılamadı"**: `TrendyolGo:BaseUrl` is likely wrong or unset — confirm the exact domain against developers.tgoapps.com.
- **Test Connection returns 403 even with correct Key/Secret**: check `TrendyolGo:AgentName` / `TrendyolGo:ExecutorUser` against the docs — these are required header values Trendyol Go validates independently of the Basic Auth credentials.
