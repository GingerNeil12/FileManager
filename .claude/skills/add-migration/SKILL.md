---
name: add-migration
description: Generate an EF Core migration with `dotnet ef migrations add`, then validate and fix the output before committing.
---

Generate and valiate and EF Core migration. The migration name should be passed as an argument (e.g `/add-migration AddUserTable`). If no name is provided, ask for one before proceeding.

## Step 1: Validate the name

- Must be descriptive of the changes being made (e.g., `AddUserTable`, `UpdateOrderSchema`).
- Reject: `Migration1`, `TestMigration`, `MyMigration` or anything that doesn't clearly indicate the purpose of the migration.
- If the name is invalid, ask for a new name until a valid one is provided.

## Step 2: Generate the migration

Find the project containing the `DbContext` (look for `*.csproj` files that reference `Microsoft.EntityFrameworkCore`) and run:

```
dotnet ef migrations add <MigrationName> --project <ProjectPath>
```

If the command fails, read the error output, diagnose the cause (unresolved model error, midding DbSet, amiguous context, ect),
then present the proposed fix to the user and wait for confirmation before making any changes. Do not proceed until the command succeeds.

## Step 3: Review the generated migration

Read all three generated/modified files:
- `Migrations/<timestamp>_<MigrationName>.cs` - the migration itself
- `Migrations/<timestamp>_<MigrationName>.Designer.cs` - the designer file
- `Migrations/ApplicationDbContextModelSnapshot.cs` (or equivalent) - the model snapshot

## Step 4: Validate the migration

Work through each check. For each problem found, present the issue and the proposed fix to the user and wait for their confirmation before making any changes.

***`Down()` method:***
- If generated as `throw new NotImplementedException()` - implement the reverse of the `Up()` method to allow for proper rollback.
- If the `Down()` method is empty or only contains comments, implement the reverse of the `Up()` method.
- verify every `Down()` column/table name exactly matches the corresponding `Up()` name, including case sensitivity.

**Column renames:**
- EF Core generates renames as a drop+add by default - this loses data. If you see a column being dropped and a new one being added with the same name, change it to use `RenameColumn()` instead.

**Desctructive operations:**
- Column or table drops - grep for the column/table name in `*.cs` files to confirm it is no longer referenced.
- If still referenced, block and report which files reference it.

**Data loss risks:**
- `NOT NULL` added to an existing column without a default value - add a `defaultValue` or flag if intent is ambiguous.
- Column precision reduced - flag cannot fix automatically.
- Unique constraint added to an existing column - flag cannot fix automatically as there could be duplicated data.

**Seed data:**
- Data seeded inside `Up()` - remove it and explain that it belongs in `UseAsyncSeeding()` or `HasData()` instead.

**Raw SQL:**
- `migrationBuild.Sql()` present - verify a corresponding `Down()` rollback exists; add if missing.

**Index coverage:**
- Every FK column should have a corresponding index - if missing, add it.

## Step 5: Test both directions:

Run the migrations up and then back down to verify reversibility:

```
dotnet ef database update --project <ProjectPath>
dotnet ef database update <PreviousMigration> --project <ProjectPath>
dotnet ef database update --project <ProjectPath>
```

If either direction fails, read the error, diagnose the cause, present the proposed fix to the user and wait for confirmation before making changes.

## Output:

Report:
- Migration name and files generated/modified
- Any fixes applied (with before/after for non-trivial changes)
- Any issues that require manual intervention (e.g. data backfill steps, ambiguous truncation risk)
- Confirmation that both up and down directions succeeded