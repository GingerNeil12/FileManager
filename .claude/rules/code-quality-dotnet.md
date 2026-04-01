---
alwaysApply: true
---

# Code quality - C# (.NET)

## Principles

- Functions do one thing. If it needs a section comment, extract the section.
- No magic values: extract the numbers, strings and config into named constants.
- Handle errors at the boundary. Don't catch and re-throw without adding context.
- No premature abstractions. Three similar lines > a helper used once.
- Don't add features or "improve" things beyond what is asked.
- No dead code or commented out blocks. Git has history.
- Composition over inheritance.
- All public controller methods that are mapped to a route have `ProducesResponseType` attributes mapping all possible status codes and response bodies.
- Use static Extension methods for converting from one model to another (`userDto.ToRequest()`,`userResponse.ToDto()`). These extension method should be in their own classes to allow for unit testing.

## Naming


- **Booleans**: `is`, `has` `should`, `can` prefixes (`isLoading`, `hasPermission`).
- **Factories**: `create*` (`createUser`).
- **Converters**: `to*` (`toJson`).
- **Predicates**: `is*`/`has*`.
- **Constants**: `SCREAMING_SNAKE` (`MAX_RETRIES`, `API_BASE_URL`).
- **Enums**: PascalCase members (`Status.Active`).
- **Abbreviations**: only universally known (`id`,`url`,`api`,`db`,`config`,`auth`). Acronyms as words (`userId` not `userID`).
- **Files**: PascalCase matching the type name (`UserService.cs`,`FileController.cs`).
- **Methods**: PascalCase, verb first (`GetUser`,`ValidateEmail`,`SaveFileAsync`).
- **Async methods**: always suffix with `Async` (`GetUserAsync`,`DeleteFileAsync`).
- **Interfaces**: `I` prefix (`IUserRepository`,`IFileService`).
- **Private fields**: _camelCase (`_userService`,`_logger`,`_dbContext`).
- **Parameters and local variables**: camelCase (`userId`, `fileStream`).

## Comments

- try to avoid the need for comments. Use meaningful naming as best you can.
- **WHY** never what. If the code needs a "what" comment, rename instead.
- Don't comment: obvious code, self-explanitory function names, section dividers, type info the language provides.
- No commented out code: delete it. No journal comments, git blame does this.
- API Docs (JSDoc) at module boundaries only, not every internal function.

## Code Markers

| Marker | Use |
|---|---|
| `TODO(author): desc (#issue)` | Planned work |
| `FIXME(author): desc (#issue)` | Known bugs |
| `HACK(author): desc (#issue)` | Ugly workarounds (explain the proper fix) |
| `NOTE: desc` | Non-obvious context for future readers |

Must have an owner + issue link. Don't commit TODOs you can do now. Never use `XXX`,`TEMP`,`README`.

## File Organisation

- **Usings**: system namespaces -> third-party -> project namespaces. Alphabetically within the groups.
- **Class member order**: fields -> constructors -> public properties -> public methods -> private methods.
- One class per file. File name must match the type name.
- `record` type for external DTOs (DTOs being deserialized or serialized via controllers and middleware).