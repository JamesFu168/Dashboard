import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, map, shareReplay, tap, throwError } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { AuthUser, LoginResponse } from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'dashboard.authToken';
const REFRESH_TOKEN_KEY = 'dashboard.refreshToken';
const USER_KEY = 'dashboard.authUser';

function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY);
  return raw ? JSON.parse(raw) as AuthUser : null;
}

function toAuthUser(response: LoginResponse): AuthUser {
  return {
    userId: response.userId,
    name: response.name,
    email: response.email,
    role: response.role,
    departmentId: response.departmentId
  };
}

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly api = inject(AuthApiService);

  private readonly accessTokenSignal = signal<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY));
  private readonly refreshTokenSignal = signal<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY));
  private readonly userSignal = signal<AuthUser | null>(readStoredUser());
  private refreshInFlight$: Observable<string> | null = null;

  readonly accessToken = this.accessTokenSignal.asReadonly();
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.accessTokenSignal() !== null);

  login(email: string, password: string): Observable<AuthUser> {
    return this.api.login({ email, password }).pipe(
      tap((response) => this.applySession(response)),
      map((response) => toAuthUser(response))
    );
  }

  logout(): void {
    const refreshToken = this.refreshTokenSignal();
    this.clearSession();

    if (refreshToken) {
      this.api.logout(refreshToken).subscribe({ error: () => undefined });
    }
  }

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

  private applySession(response: LoginResponse): void {
    const user = toAuthUser(response);

    this.accessTokenSignal.set(response.accessToken);
    this.refreshTokenSignal.set(response.refreshToken);
    this.userSignal.set(user);

    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  private clearSession(): void {
    this.accessTokenSignal.set(null);
    this.refreshTokenSignal.set(null);
    this.userSignal.set(null);

    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
