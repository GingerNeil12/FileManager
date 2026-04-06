import { inject } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { combineLatest, filter, map, take } from 'rxjs';

const ROLES_CLAIM = 'http://localhost/roles';

export const roleGuard = (allowedRoles: string[]) => () => {
  const auth = inject(AuthService);

  return combineLatest([auth.user$, auth.isLoading$]).pipe(
    filter(([, isLoading]) => !isLoading),
    take(1),
    map(([user]) => {
      const roles: string[] = user?.[ROLES_CLAIM] ?? [];
      return allowedRoles.some((role) => roles.includes(role));
    })
  );
};
