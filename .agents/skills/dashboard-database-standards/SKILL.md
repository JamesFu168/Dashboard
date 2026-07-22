---
name: dashboard-database-standards
description: Apply the Dashboard project's SQL Server database conventions when creating or modifying tables, date columns, scalar functions, default constraints, seed data, or Dashboard.Database.sqlproj. Use for work under DB/ that introduces or changes creation and update timestamps, especially CreatedAt and UpdatedAt.
---

# Dashboard Database Standards

Apply these rules to changes under `DB/`.

## Inspect before editing

1. Read `DB/Dashboard.Database.sqlproj` and the affected SQL files.
2. Search all files under `DB/` for related types, constraints, functions, and seed values.
3. Preserve unrelated user changes and existing naming conventions.

## Apply date and time conventions

- Use SQL Server `datetime` for date-and-time columns. Do not introduce `datetimeoffset` or timezone-bearing values.
- Use `date` when a column stores only a calendar date, such as `DueDate`.
- Treat stored `datetime` values as Taiwan local time (UTC+8).
- Reuse `[dbo].[GetTaiwanDate]()` for the current Taiwan date and time. Do not create a table-specific equivalent.
- Define the shared function as:

```sql
CREATE FUNCTION [dbo].[GetTaiwanDate]()
RETURNS datetime
AS
BEGIN
    RETURN DATEADD(HOUR, 8, GETUTCDATE());
END;
```

- Ensure `Functions\GetTaiwanDate.sql` is included as a `Build` item in `DB/Dashboard.Database.sqlproj`.

## Define timestamp columns

- Define a required creation timestamp using a named default constraint:

```sql
[CreatedAt] datetime NOT NULL
    CONSTRAINT [DF_<TABLE>_CreatedAt] DEFAULT ([dbo].[GetTaiwanDate]())
```

- Replace `<TABLE>` with the table name used by the project's constraint naming style.
- Define update timestamps as `datetime`, for example `[UpdatedAt] datetime NOT NULL`.
- Leave `UpdatedAt` management to the application update flow unless the user explicitly requests database-managed update behavior.

## Keep supporting data consistent

- Write explicit Taiwan-local seed timestamps in ISO format without `Z` or an offset, for example `'2026-07-22T16:00:00'`.
- When converting an existing UTC instant to the local representation, add eight hours rather than merely deleting its timezone suffix.
- Update all affected DB-owned scripts and avoid changing API contracts or application models unless the request includes them. Report any compatibility concern discovered outside `DB/`.

## Verify the result

1. Search `DB/` case-insensitively for `datetimeoffset` and timezone-bearing timestamp literals; expect no unintended matches.
2. Confirm each creation timestamp uses `[dbo].[GetTaiwanDate]()` and a uniquely named default constraint.
3. Confirm every new SQL object is included in `Dashboard.Database.sqlproj`.
4. Build the SQL project with SSDT in Release configuration when MSBuild with SQL tooling is available.
5. Report the files changed and build warnings or errors.
