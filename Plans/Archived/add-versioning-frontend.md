# Add versioning for the Frontend

We need to add versioning for the Frontend. We would like a similar implementation to the Backend where the appVersion and appName is set in `environment.ts` and `environment.Development.ts` as well. This will then be displayed on a new page found at the url `/version` on the app.

## Requirements

- Versioning added to the Frontend
  - Similar to the Backend in that it's configurable.
- New page on the app for displaying the version info on `/version` path.
- Testing using Jasmine.
- New page only shows the version info for the Frontend.
- Utilise Bootstrap themes for css and building the UI.

## Not in scope

- Loading version from Backend.
- No NavBar or side panel for navigation.
  - This will just be available by going directly to `/version` just now.
- No header or footer needed yet.

---

## Implementation Notes

### Deviations from original plan

| Topic | Plan | Actual | Reason |
|---|---|---|---|
| Testing framework | Jasmine | Vitest | Vitest was already configured; no Jasmine setup existed. `testing.md` and `test-writer-angular` skill updated to reflect this. |
| Route loading | Eager | Lazy (`loadComponent`) | Simplify review flagged eager import as unnecessary initial bundle cost. |
| `ChangeDetectionStrategy` | Not specified | `OnPush` added | Simplify review recommended it for this pure display component. |
| Spec assertions | Hardcoded strings | `environment.appName` / `environment.appVersion` | Simplify review flagged brittleness of hardcoded values. |

### Key decisions

- `appName = 'filemanager-frontend'`, `appVersion = '1.0.0'` to mirror backend naming convention.
- Standard Angular environment pattern: `environment.ts` (prod) replaced by `environment.development.ts` (dev) via `fileReplacements` in `angular.json`.
- Bootstrap imported globally via `styles.css` (`@import 'bootstrap/dist/css/bootstrap.min.css'`).
- Page layout: centred Bootstrap card with app name as title and version as muted text below.
- Tests run against the development environment (Angular test builder uses dev config by default).

---

## File Summary

| File | Action |
|---|---|
| `Frontend/src/environments/environment.ts` | Created |
| `Frontend/src/environments/environment.development.ts` | Created |
| `Frontend/src/app/pages/version/version.component.ts` | Created |
| `Frontend/src/app/pages/version/version.component.html` | Created |
| `Frontend/src/app/pages/version/version.component.spec.ts` | Created |
| `Frontend/src/app/app.routes.ts` | Modified |
| `Frontend/src/styles.css` | Modified |
| `Frontend/angular.json` | Modified |
| `Frontend/package.json` | Modified |
| `Frontend/package-lock.json` | Modified |
| `.claude/rules/testing.md` | Modified |
| `.claude/skills/test-writer-angular/SKILL.md` | Modified |
