import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdminUser,
  AdminCategory,
  AdminCreateCategory,
  AdminUpdateCategory,
  AdminSystemStats,
  AuditLog
} from '../models/admin.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5228/api/Admin';

  // Users
  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.apiUrl}/users`);
  }

  updateUserStatus(id: number, isActive: boolean): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/users/${id}/status`, { isActive });
  }

  updateUserRole(id: number, role: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/users/${id}/role`, { role });
  }

  // Categories
  getCategories(): Observable<AdminCategory[]> {
    return this.http.get<AdminCategory[]>(`${this.apiUrl}/categories`);
  }

  createCategory(category: AdminCreateCategory): Observable<AdminCategory> {
    return this.http.post<AdminCategory>(`${this.apiUrl}/categories`, category);
  }

  updateCategory(id: number, category: AdminUpdateCategory): Observable<AdminCategory> {
    return this.http.put<AdminCategory>(`${this.apiUrl}/categories/${id}`, category);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/categories/${id}`);
  }

  // System Stats
  getSystemStats(): Observable<AdminSystemStats> {
    return this.http.get<AdminSystemStats>(`${this.apiUrl}/stats`);
  }

  // Audit Logs
  getAuditLogs(limit: number = 200): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.apiUrl}/audit-logs?limit=${limit}`);
  }
}
