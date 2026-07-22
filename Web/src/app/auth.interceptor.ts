import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthStateService } from './features/auth/services/auth-state.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  const isAuthEndpoint = request.url.includes('/api/v1/auth/');
  const token = authState.accessToken();

  const authorizedRequest = token && !isAuthEndpoint
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authorizedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (isAuthEndpoint || error.status !== 401) {
        return throwError(() => error);
      }

      return authState.refreshAccessToken().pipe(
        switchMap((newToken) => next(request.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }))),
        catchError((refreshError) => {
          authState.logout();
          void router.navigateByUrl('/login');
          return throwError(() => refreshError);
        })
      );
    })
  );
};
