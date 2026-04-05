# Add navbar for UI

We need to add a nav bar to the top of the Frontend app. This will be viewable throughout the whole of the project and pinned to the top of the window. All items like toasts will appear below it. Currently the navbar will only have a Login button on the far right hand side of the navbar that goes no where.

## Requirements

- Navbar that is pinned to the top of the window.
- The Navbar will be viewable on all screens of the app.
- Login button on the far right hand side of the nav bar.
- All other items like toasts or tables or forms appear below the navbar and are not cut off by it.

## Not in scope

- Login page: this will be implemented in another plan.
- Any other links on the navbar.

---

## Implementation Notes

### Deviations from original plan

- **Bootstrap Icons removed**: The plan called for a Bootstrap Icon (`bi-folder2-open`) placeholder via the `bootstrap-icons` npm package. During the simplify pass, the full package (~380KB CSS) was replaced with a single inline SVG path taken directly from the Bootstrap Icons source. This kept the bundle under the 500KB budget.
- **Toast offset moved to CSS**: The toast `top` offset was initially set via an inline style (`style="top: var(--navbar-height)"`). During the simplify pass it was moved to `toast.component.css` for proper separation of concerns.
- **RouterLink used instead of `href="#"`**: The brand link uses `routerLink="/"` rather than a plain anchor to integrate with Angular's router.

---

## Files Summary

| File | Status |
| ------ | -------- |
| `Frontend/src/app/core/components/navbar/navbar.component.ts` | Created |
| `Frontend/src/app/core/components/navbar/navbar.component.html` | Created |
| `Frontend/src/app/core/components/navbar/navbar.component.spec.ts` | Created |
| `Frontend/src/app/app.ts` | Modified — added `NavbarComponent` import |
| `Frontend/src/app/app.html` | Modified — added `<app-navbar />` |
| `Frontend/src/styles.css` | Modified — added `--navbar-height` CSS variable and `body` padding-top |
| `Frontend/src/app/core/components/toast/toast.component.html` | Modified — removed inline style |
| `Frontend/src/app/core/components/toast/toast.component.ts` | Modified — added `styleUrl` |
| `Frontend/src/app/core/components/toast/toast.component.css` | Created — navbar-height top offset |
| `Frontend/package.json` | Modified — `bootstrap-icons` added then removed |
