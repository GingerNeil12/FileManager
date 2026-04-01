---
alwaysApply: true
---

# Code Quality - Angular (Typescript)

## Principles

- Functions do one thing. If it needs a section comment, extract the section.
- No magic values: extract the numbers, strings and config into named constants.
- Handle errors at the boundary. Don't catch and re-throw without adding context.
- No premature abstractions. Three similar lines > a helper used once.
- Don't add features or "improve" things beyond what is asked.
- No dead code or commented out blocks. Git has history.
- Composition over inheritance.

## Naming

- **Booleans**: `is`, `has` `should`, `can` prefixes (`isLoading`, `hasPermission`).
- **Factories**: `create*` (`createUser`).
- **Converters**: `to*` (`toJson`).
- **Predicates**: `is*`/`has*`.
- **Constants**: `SCREAMING_SNAKE` (`MAX_RETRIES`, `API_BASE_URL`).
- **Enums**: PascalCase members (`Status.Active`).
- **Abbreviations**: only universally known (`id`,`url`,`api`,`db`,`config`,`auth`). Acronyms as words (`userId` not `userID`).
- **Files**: kebab-case with type suffix (`user-profile.service.ts`,`auth.service.ts`,`admin.guard.ts`).
- **Classes**: PascalCase (`UserProfileComponent`,`AuthService`).
- **Methods and variables**: camelCase, verb first (`getUserInfo`,`validateEmail`,`handleSumbit`).
- **Event Handlers**: `handle` internally, `on*` for `@Output()`. Event emitters `handleClick` inside component `(clicked)="onClicked()` in template.
- **Signals/Observable**: suffix observables with `$` (`users$`,`authState$`).

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

- **Imports**: builtins -> external packages -> internal (`@app/...`) -> relative -> type-only imports. Blank line between groups.
- **Exports**: named over default. Export at declartion site. One component/service/class per file.
- **Class member order**: `@Input`/`@Output` -> injected services -> lifecycle hooks, public methods -> private methods.
