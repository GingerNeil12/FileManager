import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'version',
    loadComponent: () =>
      import('./pages/version/version.component').then((m) => m.VersionComponent),
  },
];
