import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthResponse } from '../models/api.models';
import { AuthStore } from './auth.store';

describe('AuthStore', () => {
  const session: AuthResponse = {
    accessToken: 'access-token',
    refreshToken: 'refresh-token',
    expiresAt: '2026-09-16T12:00:00Z',
    user: { id: 'user-id', name: 'Test User', email: 'test@example.com', role: 'User' },
  };

  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [AuthStore, provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('logs in through the configured API and persists the session', () => {
    store.login('test@example.com', 'StrongPass123!').subscribe();

    const request = http.expectOne('http://localhost:5080/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'test@example.com',
      password: 'StrongPass123!',
    });
    request.flush({ success: true, message: 'Authentication completed successfully.', data: session });

    expect(store.isAuthenticated()).toBe(true);
    expect(store.user()).toEqual(session.user);
    expect(JSON.parse(localStorage.getItem('ledgerflow.session') ?? '{}')).toEqual(session);
  });

  it('registers through the configured API', () => {
    store.register('Test User', 'test@example.com', 'StrongPass123!').subscribe();

    const request = http.expectOne('http://localhost:5080/api/auth/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      name: 'Test User',
      email: 'test@example.com',
      password: 'StrongPass123!',
    });
    request.flush({ success: true, message: 'User registered successfully.', data: session });
    expect(store.accessToken()).toBe('access-token');
  });

  it('clears the local session and revokes its refresh token on logout', () => {
    store.login('test@example.com', 'StrongPass123!').subscribe();
    http.expectOne('http://localhost:5080/api/auth/login').flush({
      success: true,
      message: 'Authentication completed successfully.',
      data: session,
    });

    store.logout();

    const request = http.expectOne('http://localhost:5080/api/auth/logout');
    expect(request.request.body).toEqual({ refreshToken: 'refresh-token' });
    request.flush({ success: true, message: 'Session ended successfully.' });
    expect(store.isAuthenticated()).toBe(false);
    expect(localStorage.getItem('ledgerflow.session')).toBeNull();
  });
});
