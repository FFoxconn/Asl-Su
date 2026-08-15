# Build Phases

Each phase lands as its own change set and must build/run cleanly before the next one starts.

- [x] **Phase 1 — Repo & tooling scaffold**: monorepo layout, backend solution (5 src + 4 test projects, empty), Vite web app, Expo mobile app, `.gitignore`, `docker-compose.yml`.
- [ ] **Phase 2 — Backend skeleton end-to-end**: `AslSuDbContext` with User/auth tables, first migration, JWT login pipeline, Swagger, Serilog console sink, `appsettings.Development.json` split (gitignored), `HealthController`; web + mobile login screens hitting `/health`.
- [ ] **Phase 3 — Core domain model**: remaining entities (Product, StoreProductInventory, Store, Category, Brand, Customer, Order, OrderItem, SyncLog, WebhookLog, BatchRequestLog), migrations, Products/StockPrice CRUD endpoints, basic Products screens.
- [ ] **Phase 4 — TrendyolGo client + config + Test Connection**: `AslSu.TrendyolGo` implementation, `TrendyolGoOptions` + user-secrets wiring, ApiSettings screen with masked fields + Test Connection, 401/403/429/5xx error mapping.
- [ ] **Phase 5 — Product sync (push)**: batch splitter, `TgoSyncStatus` tracking, push endpoint + UI.
- [ ] **Phase 6 — Stock/price sync + batch polling**: delta-only calculation, `getBatchRequestResult` polling, failures surfaced in Sync Logs.
- [ ] **Phase 7 — Sell/unsell**: with reason codes confirmed against developers.tgoapps.com.
- [ ] **Phase 8 — Order pull + persistence + upsert**: packages GET, PackageId-keyed upsert, Orders screens.
- [ ] **Phase 9 — Order status workflow**: accept/invoiced/shipped calls, local Yeni→Kabul Edildi→Hazırlanıyor→Hazırlandı→Teslim Edildi flow.
- [ ] **Phase 10 — Alternative product/substitution handling** (once confirmed against docs).
- [ ] **Phase 11 — Webhook endpoint**: signature validation, idempotent dedupe, WebhookLog, wired to order updates.
- [ ] **Phase 12 — Polling fallback background service**: 30–60s cadence, rate-limit aware.
- [ ] **Phase 13 — Background auto-sync service**: ~1 min cadence for stock/price/product deltas.
- [ ] **Phase 14 — Logging/error-handling hardening**: audit no secrets in logs/responses.
- [ ] **Phase 15 — Testing gap-fill pass**.
- [ ] **Phase 16 — Documentation finalization**.

Full design rationale lives in `ARCHITECTURE.md` and the original sign-off plan.
