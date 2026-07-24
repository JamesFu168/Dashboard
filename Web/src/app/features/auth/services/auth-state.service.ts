import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, map, shareReplay, tap, throwError } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { AuthUser, LoginResponse } from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'dashboard.authToken';
const REFRESH_TOKEN_KEY = 'dashboard.refreshToken';
const USER_KEY = 'dashboard.authUser';

/**
 * 從 LocalStorage 讀取當前儲存的使用者資訊。
 */
function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY);
  return raw ? JSON.parse(raw) as AuthUser : null;
}

/**
 * 將 API 回傳之 LoginResponse 轉換為 AuthUser 結構。
 */
function toAuthUser(response: LoginResponse): AuthUser {
  return {
    userId: response.userId,
    name: response.name,
    email: response.email,
    role: response.role,
    departmentId: response.departmentId
  };
}

/**
 * 身份驗證狀態管理服務 (AuthStateService)。
 * 利用 Angular Signals 管理當前 Access Token、Refresh Token 與使用者登入狀態。
 */
@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly api = inject(AuthApiService);

  private readonly accessTokenSignal = signal<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY));
  private readonly refreshTokenSignal = signal<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY));
  private readonly userSignal = signal<AuthUser | null>(readStoredUser());
  private refreshInFlight$: Observable<string> | null = null;

  /** 當前存取 Token (唯讀 Signal) */
  readonly accessToken = this.accessTokenSignal.asReadonly();
  /** 當前登入使用者資訊 (唯讀 Signal) */
  readonly user = this.userSignal.asReadonly();
  /** 是否為已登入狀態 (Computed Signal) */
  readonly isAuthenticated = computed(() => this.accessTokenSignal() !== null);

  /**
   * 執行登入流程。
   * @param email 使用者電子郵件
   * @param password 使用者密碼
   */
  login(email: string, password: string): Observable<AuthUser> {
    return this.api.login({ email, password }).pipe(
      tap((response) => this.applySession(response)),
      map((response) => toAuthUser(response))
    );
  }

  /**
   * 執行登出流程，清除本地 Session 並向後端撤銷 Refresh Token。
   */
  logout(): void {
    const refreshToken = this.refreshTokenSignal();
    this.clearSession();

    if (refreshToken) {
      this.api.logout(refreshToken).subscribe({ error: () => undefined });
    }
  }

  /**
   * 使用 Refresh Token 換發新的 Access Token。
   * 具備並發請求解鎖 (shareReplay) 機制，避免多個 API 同時 401 時重複觸發 refresh。
   */
  refreshAccessToken(): Observable<string> {
    if (this.refreshInFlight$) {
      return this.refreshInFlight$;
    }

    const refreshToken = this.refreshTokenSignal();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available.'));
    }

    this.refreshInFlight$ = this.api.refresh(refreshToken).pipe(
      tap((response) => {
        this.accessTokenSignal.set(response.accessToken);
        this.refreshTokenSignal.set(response.refreshToken);
        localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
        localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
      }),
      map((response) => response.accessToken),
      shareReplay(1),
      finalize(() => this.refreshInFlight$ = null)
    );

    return this.refreshInFlight$;
  }

  /**
   * 將登入回應寫入 Signals 與 LocalStorage 持久化儲存。
   */
  private applySession(response: LoginResponse): void {
    const user = toAuthUser(response);

    this.accessTokenSignal.set(response.accessToken);
    this.refreshTokenSignal.set(response.refreshToken);
    this.userSignal.set(user);

    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  /**
   * 清除記憶體與 LocalStorage 中的 Session 資料。
   */
  private clearSession(): void {
    this.accessTokenSignal.set(null);
    this.refreshTokenSignal.set(null);
    this.userSignal.set(null);

    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
