import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Budget,
  BudgetCreate
} from '../models/budget.model';

@Injectable({
  providedIn: 'root'
})
export class BudgetService {

  private http = inject(HttpClient);

  private apiUrl =
    'http://localhost:5228/api/Budget';

  create(
    budget: BudgetCreate
  ): Observable<{ message: string }> {

    return this.http.post<{ message: string }>(
      this.apiUrl,
      budget
    );
  }

  get(
    year: number,
    month: number
  ): Observable<Budget> {

    return this.http.get<Budget>(
      `${this.apiUrl}/${year}/${month}`
    );
  }

  update(
  year: number,
  month: number,
  budget: BudgetCreate
): Observable<{ message: string }> {

  return this.http.put<{ message: string }>(
    `${this.apiUrl}/${year}/${month}`,
    budget
  );
}
}