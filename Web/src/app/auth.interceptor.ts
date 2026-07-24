import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthStateService } from './features/auth/services/auth-state.service';

/**
/// HTTP 身份驗證攔截器 (Auth Interceptor)
/// 自動為出站 HTTP 請求附加 Bearer Access Token。
/// 當遭遇 401 Unauthorized 時，自動嘗試透過 Refresh Token 換發新 Access Token，並重試原請求。
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  // 判斷是否為身份驗證相關 Endpoint (免帶 Token)
  const isAuthEndpoint = request.url.includes('/api/v1/auth/');
  const token = authState.accessToken();

  // 為非 Auth 請求附加 Bearer Token
  const authorizedRequest = token && !isAuthEndpoint
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authorizedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // 非 401 錯誤或登入頁 API 錯誤時直接拋出
      if (isAuthEndpoint || error.status !== 401) {
        return throwError(() => error);
      }

      // 嘗試利用 Refresh Token 自動續期
      return authState.refreshAccessToken().pipe(
        switchMap((newToken) => next(request.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }))),
        catchError((refreshError) => {
          // 續期失敗則清除登入狀態並引導至登入頁
          authState.logout();
          void router.navigateByUrl('/login');
          return throwError(() => refreshError);
        })
      );
    })
  );
};
