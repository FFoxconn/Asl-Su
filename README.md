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

Trendyol Go credentials will be read the same way (`dotnet user-secrets` in dev, environment variables in prod) once the integration client lands in a later phase — see `docs/TRENDYOL_GO_SETUP.md`.

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

This repository is being built in small, incremental phases — see `docs/PHASES.md` for the current phase and what's next.
