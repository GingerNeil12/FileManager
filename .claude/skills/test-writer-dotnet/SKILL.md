---
name: test-writer-dotnet
description: Write comprehensive NUnit tests for new or changed code. Use automatically when .cs files are added or modified.
---

Write comprehensive NUnit tests for the C# code that was just added or modified. These will use a combination of:
- NUnit
- NSubstitute: for mocking dependencies
- TestContainers: for mocking external services like databases, blob storage.

## 1. Discover Changes

- Check `git diff` and `git diff --cached` filtered for `.cs` files to find new or changed code.
- Read each changed file to understand the behavior and logic of the code.
- If no existing test file exists, create `<ProjectName>.Tests/<same folder structure>/<ClassName>Tests.cs` for the new code.
- Place new test files in `<ProjectName>.Tests/<same folder structure>/` to mirror the source code structure.

## 2. Analyze every code path

For each new or modified method map out:

- **Happy Path**: The expected flow when all inputs are valid.
- **Edge Cases**: null arguments, empty collections, boundary values etc.
- **Error paths**: invalid inputs, missing entities, exceptions etc.
- **`Result<TValue, TError>`**: If the method returns a `Result`, ensure tests cover both success and failure cases.
- **Async**: successful resolution, exception propagation, cancellation etc.
- **Concurrency**: parallel calls on shared state.
- **Integration points**: interactions with dependencies, correct parameters passed etc.

## 3. Write Tests

For EACH scenario identified above, write a test. No skipping.

### Structure

- **One assert per test**: Each test should verify one specific behavior or outcome.
- **Assert.EnterMultipleScope** use this if there is multiple asserts being done in the test that are connected.
- **Arrange-Act-Assert**: Follow this pattern for clarity using `//Arrange`, `//Act`, and `//Assert` comments for each section.
  - Arrange: Set up necessary objects, mocks, and inputs.
  - Act: Call the method under test.
  - Assert: Verify the expected outcome.
- **nullable disable** set `#nullable disable` at the top of the test files. This means there is no need to deal with nullable using `null!` or `?`.

### What to test

**Services / business logic**
- Every branch (if/else, switch, ternary).
- Every thrown exception with exact type and message.
- Return value shape
- Side effects: calls to dependencies with correct arguements.

**Controllers/ API endpoints**
- Success response: status codes and body state
- Validation errors: each field mssing, wrong type, out of range etc.
- Authentication and Authorization failures

**Data layer (repositories / EF core)**
- CRUD operations return correct data
- Unique constraints reject duplicates
- Cascade deletes behave as expected
- Transactions roll back on failures

**Async**
- Successful resolution
- Exception propagation
- Cancellation token respected

### Mocking rules

- Never mock the class under test

### Naming

`MethodName_Scenario_ExpectedBehaviour`: example: `GetFile_WhenUserDoesNotOwnFile_ReturnsUnauthorized`

At least one happy path and one failure path test per public method

### Assertions

Use NUnits constraint model only: never legacy `Assert.AreEqual`

```csharp
Assert.That(result, Is.EqualTo(expected));
Assert.That(result.IsSuccess, Is.True);
```

### `Result<TValue, TError>` return types

Always assert the discriminator first then the value or error
```csharp
// Success path
Assert.That(result.IsSuccess, Is.True);
Assert.That(result.Value, Is.EqualTo(expected));

// Failure path: asserting IsSuccess is not sufficient
Assert.That(result.IsSuccess, Is.False);
Assert.That(result.Error, Is.InstanceOf<ExpectedErrorType>());
```

### NSubsititute conventions

- All mocks prefixed with `mock`: `mockUserRepository`, `mockLogger`
- Name the class under test `_sut`: instantiate it in `[SetUp]`
- Always assert arguements on received calls: `Received(1)` alone is not sufficient
- Use `Arg.Any<T>()` sparingl: prefer specific matchers so tests catch wrong values being passed
- Setup return values: `mockService.GetById(id).Returns(entity)`
- Verify calls: `mockService.Received(1).GetById(Arg.Is<Guid>(id => fileId))`

### EF Core TestContainers

- **Never use `InMemoryDatabase`**: it does not enforce constraints, unique indexes or cascades
- Spin up a real database in a container via TestContainers
- Start container in `[OneTimeSetUp]`, stop in `[OneTimeTearDown]`
- Run `MigrateAsync()` before each test class: reset state between tests via transaction rollback or re-migration
- Build `DbContextOptions` from the containers connection string: never from app configuration

### Controller / Endpoint integration tests

- Use `WebApplicationFactory<Program>`: never instantiate controllers directly
- Override services and configuration via `ConfigureWebHost`

## 4. Verify

- Run: `dotnet test --filter "FullQualifiedName~ClassName"` against the specific test class
- Temporarily break the code (change return value or condidition): confirm at least one test fails
- If no tests fails when code is broken the tests are useless: rewrite them
- Check coverage: every public method has at least one test, every branch is exercised

## Output

- Complete, compilable test file: not snippets
- Tests grouped by the method they cover
- Breif summary: how many tests, what scenarios covered and any gaps you couldn't cover and why

## What not to test

- Private methods: test them through the public API that uses them
- EF Core migrations and scaffolded boiler plate
- ASP.NET framework behavior: routing resolution, DI wiring etc
- Reflection: avoid entirely. Only use if genuinely needed, flag this in the output for reviewing before committing.