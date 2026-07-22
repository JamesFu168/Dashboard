# Dashboard Kanban Database

This folder stores database-owned artifacts for the Kanban dashboard.

- `scripts/001_schema.sql`: Azure SQL compatible table schema.
- `scripts/002_seed.sql`: Initial departments, users, cards, and tasks.

The API project also contains the EF Core model. Once the EF CLI is available, create the first migration from the repository root:

```powershell
dotnet ef migrations add InitialCreate --project API --startup-project API
```
