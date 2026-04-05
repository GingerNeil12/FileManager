---
name: test-writer-angular
description: A skill to write tests for Angular applications.
---

Write comprehensive Vitest tests for the Angular code that was added or modified.

## 1. Discover what changed

- Check `git diff` and `git diff --cached` filtered to `.ts` and `.html` files under the `Frontend` directory to identnify what code was added or modified.
- Read each chnaged file to understand the behavior of the code and the expected output.
- If no existing spec exists, create `<name>.component.spec.ts` or `<name>.service.spec.ts` in the same directory as the component or service before writing tests.

## 2. Analyze every code path

For each new or modified method or component map out:

- **Happy path**: The expected flow when everything works correctly.
- **Edge cases**: Empty inputs, null values, undefined inputs, boundary values etc.
- **Null/undefined**: missing `@Input()` values, missing HTTP response fields, etc.
- **Type coercion traps**: Typescript narrowing failures, unexpected falsy values etc.
- **Error paths**: HTTP errors, thrown exceptions, failed observables etc.
- **State transitions**: loading -> populated, loading -> error etc.
- **Async**: Observable completions, Promise resolutions, setTimeouts etc.
- **Integration points**: interactions with services, child components, external libraries, `@Output()` events etc.

## 3. Write tests

For EACH scenario identified above, write a Jasmine test. No skipping.

### Structure

- **One assertion per test**: If a test name needs "and", split into two tests.
- **Arrange-Act-Assert**: Set up the test data and environment, execute the code being tested, then verify the results. Use `//Arrange`, `//Act`, `//Assert` comments to separate these sections.

### What to test

**Components**
- Renders without crashing with required '@Input()' values.
- Renders correct content for each state: loading, error, empty, populated.
- Each user interaction triggers the correct handler (click, submit, etc).
- Each conditional branch shows/hides the correct elements.
- `@Output()` events emit the correct values on the correct trigger.

**SErvices**
- Correct HTTP method, URL, request body and headers for each call.
- Response mapping: raw HTTP response correctly transformed to typed model.
- Error responses: 'ProblemDetails` shape correctly handled and propagated.
- Each distinct HTTP status code path is tested (200, 400, 404, 500 etc).

**Async**
- Observable success and error paths.
- Timer and debounce behaviour via `fakeAsync`/`tick`.
- Promise resolution and rejection via `async`/`await` with `fixture.whenStable()`.

### Mocking rules

- Prefer `HttpClientTestingModule` over manual HTTP mocks for services.
- Use `vi.spyOn` at system boundaries only: never spy on the service under test itself.
- Use `vi.fn()` for standalone mock functions.
- Always assert spy was called with expected arguments: `.toHaveBeenCalledWith(...)` not just `.toHaveBeenCalled()`.
- Reset mocks between tests via `vi.clearAllMocks()` in `afterEach` to prevent state leakage.
- Reset state between tests: no shared Observable subscriptions leaking across specs.

## File structure

- spec files in same directory as source, same name with `.spec.ts` suffix.
- one top-level `describe` per component/service.
- nest inner `describe` blocks by method or scenario
- `beforeEach` for setup; `afterEach` for cleanup if needed.

## Naming

Test descriptions use sentence case: `'should return empty array when input is empty'`

Nest `describe` blocks: outer by component/service, inner by method or scenario

## Component tests

- Always call `fixture.detectChanges()` after any state change: Angular does not update the DOM automatically in tests.
- Subscribe to `@Output()` EventEmitters before the triggered action, assert emitted value after.

## Service tests

- Use `HttpClientTestingModule` for every service making HTTP calls: no manual `HttpClient` mocks.
- Use `HttpTestingController.expectOne(url)`: assert URL, HTTP method and request body if applicable.
- Always include `httpTestingController.verify()` in `afterEach` to ensure no unexpected HTTP calls were made.
- Always `.flush()` or `.error()` requests: never leave them pending.

## Async tests

- Use `fakeAsync`/`tick()` for timers, debounce and observable delays.
- Use `async`/`await` with `fixture.whenStable()` for Promise-baseed async in component tests.
- Never use `setTimeout` in tests: use `fakeAsync`/`tick()` instead for deterministic timing.

## 4. Verify

- Run `ng test --include="**/path/to/file.spec.ts --watch=false"`
- Temporarily break the code (change a return value or condition): confirm at least one test fails.
- If no tests fail when code is broken, the tests are useless: rewrite them.
- Check coverage: every public method has at least one test, every branch is exercised.

## Output

- Complete, runnable spec file: not snippets.
- Tests grouped by the method or scenario they cover.
- A breif summary: how many tests, what scenarios covered, any gaps you couldn't cover and why.

## What not to test

- Private methods: test through the public API that uses them.
- Generated Typescript DTOs and scaffolded boilder plate.
- Angular framework behavior: DI resolution, router wiring.
- Reflection: avoid entirely. If genuinely unavoidable, flag it in the output for review before committing.