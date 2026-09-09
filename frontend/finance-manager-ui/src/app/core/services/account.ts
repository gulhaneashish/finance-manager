import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account } from '../models/account.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5228/api/Account';

  getAll(): Observable<Account[]> {
    return this.http.get<Account[]>(
      this.apiUrl
    );
  }

  // Active accounts only
  // Used in Transaction / Transfer dropdowns
  getActive(): Observable<Account[]> {
    return this.http.get<Account[]>(
      `${this.apiUrl}/active`
    );
  }

  getById(id: number): Observable<Account> {
    return this.http.get<Account>(
      `${this.apiUrl}/${id}`
    );
  }

  create(account: {
    name: string;
    accountType: string;
    openingBalance: number;
    creditLimit?: number | null;
  }): Observable<Account> {

    return this.http.post<Account>(
      this.apiUrl,
      account
    );
  }

  update(
    id: number,
    account: {
      name: string;
      accountType: string;
      openingBalance: number;
      creditLimit?: number | null;
      isActive: boolean;
    }
  ): Observable<Account> {

    return this.http.put<Account>(
      `${this.apiUrl}/${id}`,
      account
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }

  
}