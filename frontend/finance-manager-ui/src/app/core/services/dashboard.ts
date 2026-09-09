import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { AccountSummary, CategorySpending, DashboardSummary, MonthlyCashFlow, SavingsInvestmentSummary } from '../models/dashboard.model';
import { LoanDebtSummary } from '../models/loan-debt.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5228/api/Dashboard';

  getSummary(
    year: number,
    month: number
  ): Observable<DashboardSummary> {

   const params = new HttpParams()
      .set('year', year)
      .set('month', month);

    return this.http.get<DashboardSummary>(
      `${this.apiUrl}/summary`,
      { params }
    );
  }

  getCategorySpending(
  year: number,
  month: number
): Observable<CategorySpending[]> {

  const params = new HttpParams()
    .set('year', year)
    .set('month', month);

  return this.http.get<CategorySpending[]>(
    `${this.apiUrl}/category-spending`,
    { params }
  );
}

getMonthlyCashFlow(
  year: number
): Observable<MonthlyCashFlow[]> {

  const params = new HttpParams()
    .set('year', year);

  return this.http.get<MonthlyCashFlow[]>(
    `${this.apiUrl}/monthly-cash-flow`,
    { params }
  );
}
getLoanDebt(): Observable<LoanDebtSummary> {
  return this.http.get<LoanDebtSummary>(
    `${this.apiUrl}/loan-debt`
  );
}
getAccounts(): Observable<AccountSummary[]> {
  return this.http.get<AccountSummary[]>(
    `${this.apiUrl}/accounts`
  );
}
getSavingsInvestments(
  year: number,
  month: number
): Observable<SavingsInvestmentSummary> {

  const params = new HttpParams()
    .set('year', year)
    .set('month', month);

  return this.http.get<SavingsInvestmentSummary>(
    `${this.apiUrl}/savings-investments`,
    { params }
  );
}
}