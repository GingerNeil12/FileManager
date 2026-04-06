import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { catchError, throwError } from 'rxjs';

const UNAUTHORIZED = 401;

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === UNAUTHORIZED) {
        auth.logout({ logoutParams: { returnTo: window.location.origin } });
      }
      return throwError(() => error);
    })
  );
};
