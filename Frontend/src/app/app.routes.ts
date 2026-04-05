import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'version',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/version/version.component').then((m) => m.VersionComponent),
  },
];
