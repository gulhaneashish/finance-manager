import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AccountSummary,
  CategorySpending,
  DashboardFilter,
  DashboardSummary,
  MonthlyCashFlow,
  SavingsInvestmentSummary
} from '../models/dashboard.model';
import { LoanDebtSummary } from '../models/loan-debt.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5228/api/Dashboard';

  private buildParams(filter?: DashboardFilter | { year?: number; month?: number }): HttpParams {
    let params = new HttpParams();
    if (!filter) return params;

    const f = filter as DashboardFilter;
    if (f.period) params = params.set('period', f.period);
    if (f.startDate) params = params.set('startDate', f.startDate);
    if (f.endDate) params = params.set('endDate', f.endDate);
    if (f.year !== undefined && f.year !== null) params = params.set('year', f.year.toString());
    if (f.month !== undefined && f.month !== null) params = params.set('month', f.month.toString());

    return params;
  }

  getSummary(
    filterOrYear?: DashboardFilter | number,
    month?: number
  ): Observable<DashboardSummary> {
    let filter: DashboardFilter | undefined;
    if (typeof filterOrYear === 'number') {
      filter = { year: filterOrYear, month };
    } else {
      filter = filterOrYear;
    }

    const params = this.buildParams(filter);

    return this.http.get<DashboardSummary>(
      `${this.apiUrl}/summary`,
      { params }
    );
  }

  getCategorySpending(
    filterOrYear?: DashboardFilter | number,
    month?: number
  ): Observable<CategorySpending[]> {
    let filter: DashboardFilter | undefined;
    if (typeof filterOrYear === 'number') {
      filter = { year: filterOrYear, month };
    } else {
      filter = filterOrYear;
    }

    const params = this.buildParams(filter);

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
    filterOrYear?: DashboardFilter | number,
    month?: number
  ): Observable<SavingsInvestmentSummary> {
    let filter: DashboardFilter | undefined;
    if (typeof filterOrYear === 'number') {
      filter = { year: filterOrYear, month };
    } else {
      filter = filterOrYear;
    }

    const params = this.buildParams(filter);

    return this.http.get<SavingsInvestmentSummary>(
      `${this.apiUrl}/savings-investments`,
      { params }
    );
  }
}