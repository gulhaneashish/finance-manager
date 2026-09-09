import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import {
  Loan,
  LoanCreate,
  LoanPaymentCreate,
  LoanUpdate
} from '../models/loan.model';

@Injectable({
  providedIn: 'root'
})
export class LoanService {

  private http = inject(HttpClient);

  private apiUrl = 'http://localhost:5228/api/Loan';

  getAll() {
    return this.http.get<Loan[]>(this.apiUrl);
  }

  create(loan: LoanCreate) {
    return this.http.post<Loan>(
      this.apiUrl,
      loan
    );
  }

  addPayment(
    loanId: number,
    payment: LoanPaymentCreate
  ) {
    return this.http.post<Loan>(
      `${this.apiUrl}/${loanId}/payments`,
      payment
    );
  }

  update(
  loanId: number,
  loan: LoanUpdate
) {
  return this.http.put<Loan>(
    `${this.apiUrl}/${loanId}`,
    loan
  );
}
}