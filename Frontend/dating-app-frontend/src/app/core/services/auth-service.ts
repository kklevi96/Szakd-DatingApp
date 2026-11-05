import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  RegisterRequest,
  RegisterResponse
} from '../models/auth.models';

export interface UserMeResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {private http = inject(HttpClient);

  private baseUrl = environment.apiUrl;

  private loggedIn = signal(Boolean(this.getToken()));

  readonly isLoggedIn = this.loggedIn.asReadonly();

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.baseUrl}/auth/register`,
      data
    );
  }

  login(data: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/login`, data);
  }

  getMe(): Observable<UserMeResponse> {
    return this.http.get<UserMeResponse>(`${this.baseUrl}/auth/me`);
  }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
    this.loggedIn.set(true);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    this.loggedIn.set(false);
  }}
