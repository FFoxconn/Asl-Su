# Asl-Su

Monorepo for the Asl-Su product/order management system with a Trendyol Go Market integration.

## Structure

- `backend/` — ASP.NET Core Web API (C#, EF Core, SQL Server, JWT). Owns all business logic and is the only component that talks to Trendyol Go.
- `web/` — React + TypeScript responsive admin panel, calls the backend API.
- `mobile/` — React Native + Expo + TypeScript app (Android/iOS), calls the backend API.
- `database/` — local dev SQL scripts (EF Core migrations in `backend/` are the source of truth for schema).
- `docs/` — architecture notes, phased build plan, and Trendyol Go setup guide.

## Local development

### Backend

First-time setup — the app fails fast at startup if these aren't set (never put them in `appsettings.json`):

```
cd backend/src/AslSu.Api
dotnet user-secrets set "Jwt:SigningKey" "<a long random string>"
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=AslSu;User Id=sa;Password=<your local SQL Server password>;TrustServerCertificate=True"

# Optional: seeds one admin account on first run against an empty Users table (Development only)
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com"
dotnet user-secrets set "Seed:AdminPassword" "<a password>"
```

Then, with `docker-compose up -d` running for SQL Server:

```
cd backend
dotnet build
dotnet run --project src/AslSu.Api
```

Migrations apply automatically on startup in Development. Swagger UI is at `/swagger`.

Trendyol Go credentials are read the same way — `dotnet user-secrets` in dev, environment variables in prod — see `docs/TRENDYOL_GO_SETUP.md` for the full list of `TrendyolGo:*` keys and a before-going-live checklist of the endpoint paths that still need confirming against developers.tgoapps.com.

### Web

```
cd web
cp .env.example .env   # points VITE_API_BASE_URL at your local backend
npm install
npm run dev
```

### Mobile

```
cd mobile
cp .env.example .env   # points EXPO_PUBLIC_API_BASE_URL at your local backend
npm install
npx expo start
```

### Database

```
docker-compose up -d
```

Brings up a local SQL Server instance for development.

## Status

All 16 planned build phases are complete: product/stock/price/order management, the full Trendyol Go Market integration (product sync, stock/price sync, sell/unsell, order pull, order status workflow, webhook + polling fallback, background auto-sync), courier management, and a hardened/tested/documented backend. See `docs/PHASES.md` for the phase-by-phase history and `docs/TRENDYOL_GO_SETUP.md` for the small number of Trendyol Go endpoint paths/schemas that are config-gated pending confirmation against developers.tgoapps.com before going live.
