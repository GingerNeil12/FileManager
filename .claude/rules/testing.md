---
alwaysApply: true
---

# Testing

## Shared

- Write tests that verify behavior, not implementation details.
- Run the specific test file after the changes, not the full suite - faster feedback.
- If a test is flaky, fix or delete it. Never retry to make it pass.
- Prefer real implementations over mocks. Only mock at system boundaries.
- One assertion per test. If the name need "and" aplit it.
- Arrange-Act-Assert structure. No logic (if/else, loops etc) in tests. Add `// Arrange`, `// Act` and `// Assert` comments to the respective parts.
- Never assert a mock was called without checking its arguements.

## C# (.NET - NUnit)

**Naming and structure**
- Test methods: `MethodName_Scenario_ExpectedResult` - `GetUser_WhenUserDoesNotExist_ReturnsNotFound`.
- Test classes: `[TestFixture]` one per class under test, named `ClassNameTests.cs`.
- Per-test setup/teardown: `[SetUp]`/`[TearDown]`.
- Class level setup/teardown: `[OneTimeSetUp]`/`[OneTimeTearDown]`.
- Use `[TestCase]`/`[TestCaseSource]` for parameterized tests instead of duplicating test methods.

**Assertions**
- Use NUnits constraint model: `Assert.That(result, Is.Equalto(expected))`. Do not use legacy `Assert.AreEqual`.
- **Result<TValue,TError> tests**: always assert the discriminator first, then the value or error type. Assert the specific error kind on failure paths. `Assert.That(result.IsSuccess, Is.False)` alone is not sufficient.

**Mocking (NSubtitute)**
- Use NSubstitute for all mocking - `var mockUserRepository = Substitute.For<IUserRepository>()`.
- Prefix all substitutes with `mock` - `mockFileService`,`mockLogger` etc
- Configure return values with `Returns`: `mockFileService.GetFile(id).Returns(file)`.
- Assert received calls with `Received()`: `mockUserService.Received(1).GetById(id)`.
- Always assert arguments on received calls: `mockFileService.Received(1).DeleteAsync(Arg.Is<Guid>(id => id == fileId))` not just `Received(1)`.
- Use `Arg.Any<T>()` sparingly. Prefer specific argument matchers so tests catch wrong values being passed.

**EF Core - TestContainers**
- Use TestContainers to spin up a real database container for all tests that involve EF Core or Azurite. do not use `UseInMemoryDatabase`. It does not enforce relational constraints, unique indexes or cascades producing tests that pass against a fake database and fail in production.
- Start the container in `[OneTimeSetUp]` and stop it in `[OneTimeTearDown]` to share it across tests in the fixture.
- Run `MigrateAsync()` against the container before each test class. Reset database state between tests using a transaction rollback or by re-running migrations.
- Build `DbContextOptions` from the containers connection string. Do not use app configuration.

**Integration**
- Use `WebApplicationFactory<Program>` for endpoint/controller integration tests.
- Override services and configuration by `ConfigureWebHost`.

## Angular (Typescript - Jasmine + TestBed)

**Naming**
- Test descriptions use sentence case: `should return empty array when input is empty`.
- Nest `describe` blocks by component/service then by method or scenario.

**Component tests**
- Always call `fixture.detectChanges()` after state changes. Angular does not update the DOM automatically in tests.
- Test `@Output()` EventEmitters by subscribing before the triggering action and asserting the emitted value.

**Service Tests**
- Use `HttpClientTestingModule` for any service making HTTP calls. Use `HttpTestingController` to assert request urls, methods and bodies, not just the service method completed.
- Flush or error requests in `afterEach` via `httpTestingController.verify()` to catch unexpected requests.

**Async**
- Use `fakeAsync`/`tick` for testing timers, debounce and Observable delays.
- Use `async`/`await` with `fixture.whenStable()` for premise-base async in component tests.