# Trendyol Go Setup

Status: partial — this file is filled in incrementally as each phase in `PHASES.md` lands. Sections not yet implemented are marked accordingly.

## Where credentials come from

Log in to the Trendyol Go partner panel at **partner.tgomarket.com** → **Kullanıcı Bilgileri** → **Entegrasyon Süreci**. That screen shows:

- **Satıcı ID (Cari ID)** — your Supplier ID
- **API Key**
- **API Seçret** (API Secret)

These three values authenticate every backend call to Trendyol Go. Rotate them from that same screen if they're ever exposed (e.g. shared in a screenshot or chat).

## Where credentials go in this project

Credentials are never committed to the repository and never hardcoded. *(Wiring described below lands in Phase 4 — `AslSu.TrendyolGo` + configuration.)*

- **Local development**: `dotnet user-secrets` on `AslSu.Api`, e.g.
  ```
  dotnet user-secrets set "TrendyolGo:SupplierId" "..."
  dotnet user-secrets set "TrendyolGo:ApiKey" "..."
  dotnet user-secrets set "TrendyolGo:ApiSecret" "..."
  ```
- **Production**: environment variables (`TrendyolGo__SupplierId`, `TrendyolGo__ApiKey`, `TrendyolGo__ApiSecret`) or a secret manager (e.g. Azure Key Vault) — same configuration binding either way.
- `appsettings.json` only ever contains non-secret structure (base URL, timeouts) — never the key/secret/supplier id.

## Testing the connection

*(Not yet implemented — lands in Phase 4.)* Once wired, the web admin panel's **API Ayarları** screen will have a "Test Bağlantısı" button that calls the backend, which calls Trendyol Go with the configured credentials and reports back a plain-language result (success, or which of Key/Secret/Supplier ID/headers/rate-limit/Trendyol-service-issue is likely wrong).

## Product / stock / price sync

*(Not yet implemented — lands in Phases 5–7.)*

## Order sync

*(Not yet implemented — lands in Phases 8–10.)*

## Webhook

*(Not yet implemented — lands in Phase 11.)*

## Troubleshooting

*(Filled in as real failure modes are handled, from Phase 4 onward.)*
