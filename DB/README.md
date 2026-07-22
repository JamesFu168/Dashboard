# Dashboard Kanban Database

This folder stores database-owned artifacts for the Kanban dashboard.

- `Tables/*.sql`: Azure SQL compatible table definitions, one table per file.
- `Sequences/*.sql`: Sequence objects used by integer primary keys.
- `scripts/002_seed.sql`: Initial departments, users, cards, and tasks.
- `Dashboard.Database.sqlproj`: Visual Studio SQL database project that builds the schema into a DACPAC for SqlPackage deployment.

Build the DACPAC from Visual Studio, or with MSBuild on a machine that has SSDT installed:

```powershell
msbuild .\DB\Dashboard.Database.sqlproj /p:Configuration=Release
```

Publish the generated DACPAC with SqlPackage:

```powershell
SqlPackage /Action:Publish /SourceFile:.\DB\bin\Release\Dashboard.Database.dacpac /TargetConnectionString:"<connection string>"
```

The seed script is included in the project as a supporting file, but it is not wired into automatic publish because the current inserts are intended for a fresh database.

The API project also contains the EF Core model. Once the EF CLI is available, create the first migration from the repository root:

```powershell
dotnet ef migrations add InitialCreate --project API --startup-project API
```
