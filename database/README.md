# Database

EF Core migrations under `backend/src/AslSu.Infrastructure/Migrations` are the source of truth for schema — do not hand-edit the schema outside of migrations.

`scripts/` holds optional local-dev-only helpers (seed data, resets). Nothing here is applied automatically; run scripts manually against your local SQL Server instance when needed.
