import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { MonthlyReport } from '../models/monthly-report.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MonthlyReportService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/MonthlyReport`;

  getReport(
    year: number,
    month: number
  ): Observable<MonthlyReport> {

    return this.http.get<MonthlyReport>(
      `${this.apiUrl}?year=${year}&month=${month}`
    );
  }
}