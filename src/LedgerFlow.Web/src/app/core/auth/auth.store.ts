import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiMessageResponse, ApiResponse, AuthResponse, User } from '../models/api.models';

const STORAGE_KEY = 'ledgerflow.session';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}api/auth`;
  private readonly session = signal<AuthResponse | null>(this.readSession());
  readonly user = computed<User | null>(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => !!this.session()?.accessToken);
  readonly accessToken = computed(() => this.session()?.accessToken ?? null);

  login(email: string, password: string) {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, { email, password })
      .pipe(map((response) => response.data), tap((session) => this.save(session)));
  }

  register(name: string, email: string, password: string) {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, { name, email, password })
      .pipe(map((response) => response.data), tap((session) => this.save(session)));
  }

  logout(): void {
    const refreshToken = this.session()?.refreshToken;
    this.session.set(null);
    localStorage.removeItem(STORAGE_KEY);
    if (refreshToken) {
      this.http
        .post<ApiMessageResponse>(`${this.baseUrl}/logout`, { refreshToken })
        .subscribe({ error: () => undefined });
    }
  }

  private save(session: AuthResponse): void {
    this.session.set(session);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  private readSession(): AuthResponse | null {
    try {
      const value = localStorage.getItem(STORAGE_KEY);
      return value ? JSON.parse(value) as AuthResponse : null;
    } catch { return null; }
  }
}
