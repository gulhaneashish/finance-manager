import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { Observable } from 'rxjs';

import {
  LoginRequest,
  LoginResponse,
  AuthUser
} from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class Auth {

  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private readonly apiUrl =
    'http://localhost:5228/api/User';

  login(
    request: LoginRequest
  ): Observable<LoginResponse> {

    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      request
    );
  }

  getToken(): string | null {

    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }

    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  logout(): void {

    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    localStorage.removeItem('token');
  }

  getCurrentUser(): Observable<AuthUser> {

    return this.http.get<AuthUser>(
      `${this.apiUrl}/me`
    );
  }
}