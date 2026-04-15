# Plan: Specification Pattern — Base Class, DepartmentSpecification, DepartmentRepository

## Context

The repository layer currently has `DepartmentRepository.ExecuteAsync` throwing `NotImplementedException` because the Specification pattern infrastructure doesn't exist yet. The pattern will be used by future workflow features to pass filtering, ordering, and pagination intent into repositories without coupling callers to EF Core.

This ticket covers only the infrastructure and the Department slice. No controller or workflow code is changed.

---

## Confirmed Design Decisions

| Decision | Choice |
|---|---|
| Base class features | Criteria, Includes, Ordering (asc + desc), Pagination |
| `DepartmentSpecification` filters | Name contains-search only (ID lookup is a separate method) |
| Default ordering | Name ascending |
| Pagination model | `PageNumber` + `PageSize` on the base class |
| Query building | `SpecificationEvaluator<T>` utility in Persistence layer |

---

## Files to Create / Modify

| File | Action |
|---|---|
| `src/FileManager.Application/Common/Models/Specifications/Specification.cs` | Modify — implement abstract base |
| `src/FileManager.Application/Common/Models/Specifications/DepartmentSpecification.cs` | Modify — implement concrete spec |
| `src/FileManager.Persistence/Specifications/SpecificationEvaluator.cs` | Create — IQueryable builder |
| `src/FileManager.Persistence/Repositories/DepartmentRepository.cs` | Modify — implement `ExecuteAsync` |
| `tests/FileManager.Application.Tests/Specifications/DepartmentSpecificationTests.cs` | Create |
| `tests/FileManager.Persistence.Tests/Repositories/DepartmentRepositoryTests.cs` | Create |

---

## Implementation

### 1. `Specification<T>` (Application layer)

Abstract base class. All setters are `private`; subclasses mutate state only through the protected helper methods.

```csharp
public abstract class Specification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 20;

    protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;
    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected void ApplyOrderBy(Expression<Func<T, object>> expr) => OrderBy = expr;
    protected void ApplyOrderByDescending(Expression<Func<T, object>> expr) => OrderByDescending = expr;
    protected void ApplyPaging(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
```

### 2. `DepartmentSpecification` (Application layer)

- Constructor: `(string? nameSearch, int pageNumber, int pageSize)`
- Applies name contains-filter when `nameSearch` is non-null/non-whitespace; otherwise no criteria (returns all)
- Default ordering: `x => x.Name` ascending
- Always calls `ApplyPaging`

```csharp
public DepartmentSpecification(string? nameSearch, int pageNumber, int pageSize)
{
    if (!string.IsNullOrWhiteSpace(nameSearch))
        AddCriteria(x => x.Name.Contains(nameSearch));

    ApplyOrderBy(x => x.Name);
    ApplyPaging(pageNumber, pageSize);
}
```

### 3. `SpecificationEvaluator<T>` (Persistence layer)

Static helper that converts a `Specification<T>` into an `IQueryable<T>`. Keeps repository code clean.

```csharp
public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, Specification<T> spec)
    {
        IQueryable<T> query = inputQuery;

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        if (spec.OrderBy is not null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            query = query.OrderByDescending(spec.OrderByDescending);

        return query;
    }
}
```

### 4. `DepartmentRepository.ExecuteAsync` (Persistence layer)

1. Build base `IQueryable` with `AsNoTracking()`
2. Pass through `SpecificationEvaluator<Department>.GetQuery(...)`
3. Count total items (before pagination)
4. Apply `Skip` / `Take` from spec
5. Return `QueryResponse<Department>`

```csharp
public async Task<QueryResponse<Department>> ExecuteAsync(DepartmentSpecification specification, CancellationToken ct)
{
    try
    {
        IQueryable<Department> query = SpecificationEvaluator<Department>
            .GetQuery(context.Departments.AsNoTracking(), specification);

        int totalItems = await query.CountAsync(ct);

        int skip = (specification.PageNumber - 1) * specification.PageSize;

        List<Department> items = await query
            .Skip(skip)
            .Take(specification.PageSize)
            .ToListAsync(ct);

        return new QueryResponse<Department>(
            specification.PageSize,
            specification.PageNumber,
            totalItems,
            items);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unable to execute department specification query.");
        throw;
    }
}
```

---

## Tests

### `DepartmentSpecificationTests` (Application.Tests — unit)

Tests evaluate the spec's expressions against in-memory `Department` objects to verify filter behaviour.

| Test | Scenario |
|---|---|
| `Constructor_WithNameSearch_SetsCriteriaMatchingName` | Criteria compiled expression returns true when name contains search string |
| `Constructor_WithNameSearch_SetsCriteriaNotMatchingName` | Criteria compiled expression returns false when name doesn't match |
| `Constructor_WithNullNameSearch_LeavesNullCriteria` | Criteria is null when nameSearch is null |
| `Constructor_WithWhitespaceNameSearch_LeavesNullCriteria` | Criteria is null when nameSearch is whitespace |
| `Constructor_Always_SetsOrderByName` | OrderBy is not null; OrderByDescending is null |
| `Constructor_Always_AppliesPaging` | PageNumber and PageSize match constructor args |

### `DepartmentRepositoryTests` (Persistence.Tests — TestContainers)

Follows the same pattern as `ApplicationUserRepositoryTests`: one `MsSqlContainer` per fixture, transaction rollback per test.

| Test | Scenario |
|---|---|
| `ExecuteAsync_WithNoFilter_ReturnsAllDepartments` | Seeds 3 depts, no filter → all returned |
| `ExecuteAsync_WithNameSearch_ReturnsMatchingDepartments` | Seeds 3 depts, name contains → subset returned |
| `ExecuteAsync_WithPaging_ReturnsCorrectPage` | Seeds 5 depts, pageSize=2, pageNumber=2 → items 3-4 |
| `ExecuteAsync_ReturnsCorrectTotalItems` | TotalItems reflects full count before paging |
| `ExecuteAsync_WhenExceptionOccurs_Rethrows` | Disposed context → throws |
| `ExecuteAsync_WhenExceptionOccurs_LogsError` | Disposed context → error logged |

---

## Deviations from Plan

- `Specification<T>.Includes` property changed from `List<>` to `IReadOnlyList<>` (backed by a private `_includes` field) after simplify review flagged the mutable public collection as a leaky abstraction.

---

## Verification

1. `dotnet build` — no errors or warnings
2. `dotnet test tests/FileManager.Application.Tests` — all spec unit tests pass
3. `dotnet test tests/FileManager.Persistence.Tests` — all repo integration tests pass (requires Docker for TestContainers)

---

## Files Summary

| File | Action |
|---|---|
| `src/FileManager.Application/Common/Models/Specifications/Specification.cs` | Modified |
| `src/FileManager.Application/Common/Models/Specifications/DepartmentSpecification.cs` | Modified |
| `src/FileManager.Application/Common/Interfaces/IDepartmentRepository.cs` | Modified |
| `src/FileManager.Persistence/Specifications/SpecificationEvaluator.cs` | Created |
| `src/FileManager.Persistence/Repositories/DepartmentRepository.cs` | Modified |
| `tests/FileManager.Application.Tests/Specifications/DepartmentSpecificationTests.cs` | Created |
| `tests/FileManager.Persistence.Tests/Repositories/DepartmentRepositoryTests.cs` | Created |
