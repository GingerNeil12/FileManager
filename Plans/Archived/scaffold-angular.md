# Scaffold frontend

We are needing to scaffold the Frontend as an Angular app. It is unknown if the current machine has the correct versions of npm or even ng installed so that needs to be checked/updated/installed first. This app should just be a bare bones app that uses Jasmine for the testing as per the code-quality-angular rule. Assume I know nothing about Angular.

## Requirements

- npm/ng installed to latest version.
- Barebones angular app scaffolded.
- Jasmine being used for testing as per code-quality-angular rule.
- Dockerfile using `alpine` as base image and a multistage build.
- App builds and can run.

## Out of scope

- No tests written.
- No workflows added.

---

## Implementation Notes

### Deviations from original plan

| Topic | Original expectation | Actual outcome |
| --- | --- | --- |
| Test runner | Jasmine + Karma (as per code-quality-angular rule) | Angular 21 scaffolds with **Vitest** by default. `@angular/build:unit-test` replaces Karma. The spec syntax (`describe`/`it`/`expect`) and `TestBed` API remain identical — only the runner changed. |
| Node.js | Unknown — needed checking | Not installed. Installed **Node.js LTS v24.14.1** via `winget`. |
| Angular CLI | Unknown — needed checking | Not installed. Installed **Angular CLI v21.2.6** via `npm install -g @angular/cli@latest`. |
| npm | Unknown — needed checking | Installed as part of Node.js, updated to **v11.12.1**. |
| `--browsers=ChromeHeadless` | Karma-style browser flag for CI | Not applicable with Vitest. Tests run in `jsdom` by default — no browser provider package needed. |

### Key decisions

- App scaffolded directly in `Frontend/` (mirrors `Backend/` sibling).
- Standalone components (Angular 21 default — no NgModules).
- Routing included (`src/app/app.routes.ts`).
- CSS (no preprocessor) — barebones scaffold.
- Dev server runs on port `4200` (Angular default).
- Production image: `nginx:alpine` serves the static build output.

---

## Files Summary

| File | Action |
| --- | --- |
| `Frontend/.gitkeep` | Deleted |
| `Frontend/angular.json` | Created by `ng new` |
| `Frontend/package.json` | Created by `ng new` |
| `Frontend/tsconfig.json` | Created by `ng new` |
| `Frontend/tsconfig.app.json` | Created by `ng new` |
| `Frontend/tsconfig.spec.json` | Created by `ng new` |
| `Frontend/.editorconfig` | Created by `ng new` |
| `Frontend/.gitignore` | Created by `ng new` |
| `Frontend/.prettierrc` | Created by `ng new` |
| `Frontend/src/main.ts` | Created by `ng new` |
| `Frontend/src/index.html` | Created by `ng new` |
| `Frontend/src/styles.css` | Created by `ng new` |
| `Frontend/src/app/app.ts` | Created by `ng new` |
| `Frontend/src/app/app.html` | Created by `ng new` |
| `Frontend/src/app/app.css` | Created by `ng new` |
| `Frontend/src/app/app.config.ts` | Created by `ng new` |
| `Frontend/src/app/app.routes.ts` | Created by `ng new` |
| `Frontend/src/app/app.spec.ts` | Created by `ng new` |
| `Frontend/public/favicon.ico` | Created by `ng new` |
| `Frontend/Dockerfile` | Created |
| `Frontend/nginx.conf` | Created |
| `Frontend/.dockerignore` | Created |
