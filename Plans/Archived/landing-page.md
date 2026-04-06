# Landing page

We would like to create a landing page for all non-logged in users. This page will have a CTA about storing your files securely and a professional but minimal look about it.

## Requirements

- Professional but minimal looking landing page.
- Page found when non-logged in users are veiwing the index/home page.
- You are a senior UI designer.

## Not in scope

- Home page for logged in users.

---

## Implementation Summary

### Design Decisions
| Decision | Choice |
|---|---|
| Authenticated user at `/` | Redirect to `/dashboard` |
| CTA action | `loginWithRedirect()` via Auth0 |
| Colour palette | Dark navy `#0f172a` + indigo `#6366f1` accent |
| Sections | Navbar · Hero (100vh) · Feature cards · Footer |
| Hero copy | "Your files. Secured." |
| Feature cards | Upload · Share · Control |
| App name | FileManager |

### Deviations from original plan
- `handleGetStarted()` and `handleLogin()` were merged into a single `handleLogin()` method during simplify review — both buttons trigger the same Auth0 action.
- `takeUntilDestroyed` and `DestroyRef` removed — `take(1)` already completes the stream on first emission, making them redundant.
- `.feature-icon` wrapper `<div>` removed from each feature card — `class` moved directly to `<ng-icon>`.
- CSS section comments removed — class names are self-describing.

### Files Created
| File | Purpose |
|---|---|
| `Frontend/src/app/pages/landing/landing.component.ts` | Landing page component |
| `Frontend/src/app/pages/landing/landing.component.html` | Landing page template |
| `Frontend/src/app/pages/landing/landing.component.css` | Landing page styles |
| `Frontend/src/app/pages/landing/landing.component.spec.ts` | 29 unit tests |
| `Frontend/src/app/pages/dashboard/dashboard.component.ts` | Dashboard stub (auth redirect target) |
| `Frontend/src/app/pages/dashboard/dashboard.component.html` | Dashboard stub template |

### Files Modified
| File | Change |
|---|---|
| `Frontend/src/app/app.routes.ts` | Added `/` (public) and `/dashboard` (guarded) routes |
