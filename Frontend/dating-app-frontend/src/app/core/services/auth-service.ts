import { DestroyRef, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  RegisterRequest,
  RegisterResponse,
  LoginResponse,
  AdminResponse,
  SearchPreferences
} from '../models/auth.models';

export interface UserMeResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  mbtiCode: number;
  wantsRelationship: boolean;
  wantsFriendship: boolean;
  roles: string[];
  canRequestAdmin: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private http: HttpClient, destroyRef: DestroyRef) {
    const onStorage = (event: StorageEvent) => {
      if (event.storageArea !== localStorage || (event.key !== 'token' && event.key !== null)) {
        return;
      }

      const token = this.getToken();
      if (token === this.token()) { return; }
      this.user.set(null);
      this.loggedIn.set(Boolean(token));
      sessionStorage.removeItem('hobbiesChecked');
      this.token.set(token);
    };

    window.addEventListener('storage', onStorage);
    destroyRef.onDestroy(() => window.removeEventListener('storage', onStorage));
  }

  private baseUrl = environment.apiUrl;
  private token = signal(this.getToken());
  readonly currentToken = this.token.asReadonly();

  private loggedIn = signal(Boolean(this.getToken()));

  readonly isLoggedIn = this.loggedIn.asReadonly();

  private user = signal<UserMeResponse | null>(null);
  readonly currentUser = this.user.asReadonly();

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.baseUrl}/auth/register`,
      data
    );
  }

  login(data: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, data);
  }

  getMe(): Observable<UserMeResponse> {
    const token = this.getToken();

    return this.http.get<UserMeResponse>(`${this.baseUrl}/users/me`).pipe(
      tap(user => {
        if (this.getToken() === token) {
          this.user.set(user);
        }
      })
    );
  }

  updateSearchPreferences(preferences: SearchPreferences): Observable<void> {
    const token = this.getToken();
    return this.http.patch<void>(this.baseUrl + '/users/search-preferences', preferences).pipe(
      tap(() => {
        if (this.getToken() === token) {
          this.user.update(user => user ? { ...user, ...preferences } : null);
        }
      })
    );
  }

  isAdmin(): boolean {
    return this.currentUser()?.roles.includes('Admin') ?? false;
  }

  delegateAdmin(password: string): Observable<AdminResponse> {
    return this.http.patch<AdminResponse>(
      `${this.baseUrl}/auth/DelegateAdmin`,
      { password }
    );
  }

  getAdminStatus(): Observable<AdminResponse> {
    return this.http.get<AdminResponse>(`${this.baseUrl}/auth/Admin`);
  }

  removeAdmin(): Observable<AdminResponse> {
    return this.http.patch<AdminResponse>(`${this.baseUrl}/auth/RemoveAdmin`, {});
  }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
    sessionStorage.removeItem('hobbiesChecked');
    this.user.set(null);
    this.loggedIn.set(true);
    this.token.set(token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  hasCheckedHobbies(): boolean {
    return sessionStorage.getItem('hobbiesChecked') === 'true';
  }

  markHobbiesChecked(): void {
    sessionStorage.setItem('hobbiesChecked', 'true');
  }

  logout(): void {
    localStorage.removeItem('token');
    sessionStorage.removeItem('hobbiesChecked');
    this.user.set(null);
    this.loggedIn.set(false);
    this.token.set(null);
  }
}
