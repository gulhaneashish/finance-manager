import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Investment,
  InvestmentSummary,
  InvestmentType
} from '../models/investment.model';

@Injectable({
  providedIn: 'root'
})
export class InvestmentService {

  private apiUrl = 'http://localhost:5228/api/Investment';

  constructor(private http: HttpClient) {}

  getInvestments(): Observable<Investment[]> {
    return this.http.get<Investment[]>(this.apiUrl);
  }

  getSummary(): Observable<InvestmentSummary> {
    return this.http.get<InvestmentSummary>(
      `${this.apiUrl}/summary`
    );
  }

  createInvestment(data: {
    accountId: number;
    name: string;
    investmentType: InvestmentType;
    amount: number;
    investmentDate: string;
    description?: string;
  }): Observable<any> {
    return this.http.post(
      this.apiUrl,
      data
    );
  }

  updateCurrentValue(
    id: number,
    currentValue: number
  ): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/${id}/value`,
      {
        currentValue
      }
    );
  }

  sellInvestment(
    id: number,
    data: {
      accountId: number;
      sellAmount: number;
      sellDate: string;
      description?: string;
    }
  ): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/${id}/sell`,
      data
    );
  }

  deleteInvestment(id: number): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/${id}`
    );
  }
}