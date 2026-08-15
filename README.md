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

```
cd backend
dotnet build
dotnet run --project src/AslSu.Api
```

Trendyol Go credentials and JWT signing key are read from `dotnet user-secrets` (dev) or environment variables (prod) — never from `appsettings.json`. See `docs/TRENDYOL_GO_SETUP.md`.

### Web

```
cd web
npm install
npm run dev
```

### Mobile

```
cd mobile
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
