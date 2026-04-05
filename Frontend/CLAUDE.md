# Frontend

- Every authenticated route must declare `canActivate: [authGuard]` in `app.routes.ts` — hiding from nav is not authorization.
- Never add manual `Authorization` headers to HTTP requests. `authHttpInterceptorFn` handles token attachment for all `apiBaseUrl/*` requests automatically.
