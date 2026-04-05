import { inject } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { combineLatest, filter, map, take } from 'rxjs';

export const authGuard = () => {
  const auth = inject(AuthService);

  return combineLatest([auth.isAuthenticated$, auth.isLoading$]).pipe(
    filter(([, isLoading]) => !isLoading),
    take(1),
    map(([isAuthenticated]) => {
      if (!isAuthenticated) {
        auth.loginWithRedirect();
        return false;
      }
      return true;
    })
  );
};
