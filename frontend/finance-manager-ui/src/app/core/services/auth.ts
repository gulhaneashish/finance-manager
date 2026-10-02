import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { Observable, tap } from 'rxjs';

import {
  LoginRequest,
  LoginResponse,
  AuthUser,
  RefreshTokenRequest
} from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class Auth {

  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private readonly apiUrl = 'http://localhost:5228/api/User';

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        if (isPlatformBrowser(this.platformId)) {
          this.saveTokens(response.token, response.refreshToken, response.role);
        }
      })
    );
  }

  saveTokens(token: string, refreshToken?: string, role?: string): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    localStorage.setItem('token', token);
    if (refreshToken) {
      localStorage.setItem('refreshToken', refreshToken);
    }
    if (role) {
      localStorage.setItem('role', role);
    } else {
      // Extract from token if not explicitly provided
      const extractedRole = this.extractRoleFromToken(token);
      if (extractedRole) {
        localStorage.setItem('role', extractedRole);
      }
    }
  }

  getToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }
    return localStorage.getItem('token');
  }

  getRefreshToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }
    return localStorage.getItem('refreshToken');
  }

  getRole(): string {
    if (!isPlatformBrowser(this.platformId)) {
      return 'User';
    }
    const savedRole = localStorage.getItem('role');
    if (savedRole) {
      return savedRole;
    }
    const token = this.getToken();
    if (token) {
      const extracted = this.extractRoleFromToken(token);
      if (extracted) {
        localStorage.setItem('role', extracted);
        return extracted;
      }
    }
    return 'User';
  }

  isAdmin(): boolean {
    return this.getRole().toLowerCase() === 'admin';
  }

  private extractRoleFromToken(token: string): string | null {
    try {
      const payloadBase64 = token.split('.')[1];
      if (!payloadBase64) return null;
      const json = JSON.parse(atob(payloadBase64));
      return (
        json.role ||
        json['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        null
      );
    } catch {
      return null;
    }
  }

  refreshToken(): Observable<LoginResponse> {
    const token = this.getToken() || '';
    const refreshToken = this.getRefreshToken() || '';

    const payload: RefreshTokenRequest = {
      token,
      refreshToken
    };

    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh-token`, payload).pipe(
      tap(response => {
        this.saveTokens(response.token, response.refreshToken, response.role);
      })
    );
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  logout(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('role');
  }

  getCurrentUser(): Observable<AuthUser> {
    return this.http.get<AuthUser>(`${this.apiUrl}/me`);
  }
}