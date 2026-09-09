import { Injectable, inject } from '@angular/core';

import { HttpClient, HttpParams } from '@angular/common/http';

import { ReportSummary } from '../models/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  private http = inject(HttpClient);

  private apiUrl =
    'http://localhost:5228/api/Report';

  getSummary(
    fromDate?: string,
    toDate?: string
  ) {

    let params = new HttpParams();

    if (fromDate) {
      params = params.set(
        'fromDate',
        fromDate
      );
    }

    if (toDate) {
      params = params.set(
        'toDate',
        toDate
      );
    }

    return this.http.get<ReportSummary>(
      `${this.apiUrl}/summary`,
      { params }
    );
  }

}