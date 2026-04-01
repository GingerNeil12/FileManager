---
paths:
  - "**/Migrations/**"
  - "**/Data/Migrations/**"
---

# Database EF Core

## Migrations

- **Never create migrations files by hand**: always use `dotnet ef migrations add <PascalCaseName>` to generate them. Hand written migrations will like end up with incorrect model snapshots.
- **Never modify an existing migration**: Incase the migration has been applied (dev,qa,prod) create a new migration for the correction instead.
- **Migration names must be descriptive PascalCase**: describing the schema change (`AddUserTables`,`AddEmailIndex`). Never `Migraiton1`,`Update` or `Fix`.
- **Always review the generated `Down()` method**: EF Core sometimes generates a drop+add instead of the correct inverse for renames and complex changes. Fix it before committing.
- **Test migrations in both directions**: `dotnet ef database update` then `dotnet ef database update <PreviousMigration>` before committing.
- **Never seed data in migrations**: use `UseAsyncSeeding()` or `HasData()` for seeding logic.
- **Never drop columns or tables** without first confirming the data is no longer read or written anywhere in the codebase.

## Queries & DbContext

- **Always use `AsNoTracking()`** on read-only queries. The change tracker allocates snapshots of every returned entity unnecessarily when you have no intent to save or update.
- **Avoid `await` inside loops**: use `Include()`,`Select()` or batch queries to load related data in a single round trip.
- **Prefer `Select()` projections** over loading full entities when only specific fields are needed. This avoids over-fetching.
- **Use `migrationsBuilder.Sql()` sparingly**: only for operations the migration builder cannot express (views, custom functions, raw index options). Document why its needed.
- **Keep DbContext scoped**: never register as singleton. Never store in a static feild.
- **One DbContext per request**: do not manually create `new AppDbContext()`, use DI.