# Add nav panel

## Role

You are a senior Angular engineer and UI designer.

## Feature

We are looking to add a new nav panel for navigation around the site. This should be on the left had side of the screen and collapsable. This should only be able to be seen by logged in users and should be present on all screens viewable by logged in users. When extended the nav items should have text and icons, when collapsed they should just show the icons. To begin with the only nav item that will be on the nav panel will be Settings with an appropriate icon. When clicked it should open a drop down to the right with an option to navigate to the `version` page.

## Context

We are looking to slowly start building out more functionality for users to be able to move around the app. This will take the form of the navigation panel currently and then placeholder pages in the future. Once these are in place then the back end can be updated to start serving data for the users.

## Expected Outcomes

- Navpanel created on left hand side of app.
- Navpanel only viewable by logged in users.
- Navpanel appearing on all pages for logged in users.
- Navpanel is collapsable and extendable.
- Navpanel when extended shows the text and appropriate icon of the navigation.
- Navpanel when collapsed shows on the icon.
- Collapse/Extend is an icon on the bottom of the navpanel. Not a hamburger icon.
  - This could be some form of left/right arrow that flips when collapsed/extended.
- Current entry on Navpanel is Settings with a sub menu appearing to the right when clicked. Only option on the sub menu is Version which navigates to the current `/version` end page.

---

## Implementation Notes

### Design decisions made during implementation

| Decision | Detail |
| --- | --- |
| Panel pushes content | Main area resizes via `margin-left` transition; not overlaid |
| Submenu overlay | Uses `position: fixed` with `[style.left]` bound to active sidebar width CSS var |
| Widths | 60px collapsed / 220px expanded (fixed px, not %) |
| Transition | `200ms ease-in-out` on `width` (panel) and `margin-left` (main content) |
| Toggle | Chevron icon at panel bottom — `heroChevronLeft` / `heroChevronRight` flips on state |
| Mobile | Panel always visible; stays collapsed at 60px on small screens |
| Auth visibility | `toSignal(inject(AuthService).isAuthenticated$)` in `App` — signal passed down via `@if` |
| Two-way binding | `model<boolean>` on `isExpanded` in `NavPanelComponent` — parent `App` owns the signal |
| Layout offset | Removed `body { padding-top }` from `styles.css`; replaced with `.app-shell { margin-top: var(--navbar-height) }` flex container |

### Deviations from original plan

- **`app.html` visual glitch fix**: The simplify pass identified that `[class.app-main--expanded]="isNavExpanded()"` would incorrectly apply 220px `margin-left` when the user is logged out. Fixed to `isAuthenticated() && isNavExpanded()`.
- **Removed redundant `auth` field from `App`**: Original plan injected `AuthService` into a named field. Simplified to `toSignal(inject(AuthService).isAuthenticated$, ...)` — no intermediate `auth` property needed.
- **Icon assertion strategy in tests**: `ng-reflect-name` attribute is not set for static string inputs in Angular 21. Tests for icon rendering assert against the distinctive SVG `path d` attribute values from the `@ng-icons/heroicons` package instead.

---

## Files Created / Modified / Deleted

| File | Action |
| --- | --- |
| `Frontend/src/styles.css` | Modified — removed `body { padding-top }`, added CSS vars and layout classes |
| `Frontend/src/app/app.ts` | Modified — OnPush, `toSignal` for auth, `isNavExpanded` signal, imports `NavPanelComponent` |
| `Frontend/src/app/app.html` | Modified — flex shell, conditional nav panel, `margin-left` class bindings |
| `Frontend/src/app/core/components/nav-panel/nav-panel.component.ts` | Created |
| `Frontend/src/app/core/components/nav-panel/nav-panel.component.html` | Created |
| `Frontend/src/app/core/components/nav-panel/nav-panel.component.css` | Created |
| `Frontend/src/app/core/components/nav-panel/nav-panel.component.spec.ts` | Created — 33 tests, 100% pass |
