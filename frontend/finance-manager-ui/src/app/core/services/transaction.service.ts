import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Transaction, TransactionPurpose, TransactionType } from '../models/transaction.model';
import { TransferCreate } from '../models/transfer.model';
import { TransactionList } from '../../features/transactions/transaction-list/transaction-list';
import { TransactionFilter } from '../models/transaction-filter.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {

  private http = inject(HttpClient);

  private apiUrl =
    `${environment.apiUrl}/Transaction`;

  getAll(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(
      this.apiUrl
    );
  }

  create(transaction: {
    accountId: number | null;
    fromAccountId?: number | null;
    toAccountId?: number | null;
    categoryId?: number | null;
    amount: number;
    type: TransactionType;
    description: string;
    transactionDate: string;
    purpose: TransactionPurpose;
  }): Observable<Transaction> {

    return this.http.post<Transaction>(
      this.apiUrl,
      transaction
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }

  transfer(transfer: TransferCreate): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/transfer`,
      transfer
    );
  }

  getFiltered(
    filter: TransactionFilter
  ): Observable<Transaction[]> {

    let params = new HttpParams();

    if (filter.fromDate) {
      params = params.set(
        'fromDate',
        filter.fromDate
      );
    }

    if (filter.toDate) {
      params = params.set(
        'toDate',
        filter.toDate
      );
    }

    if (filter.accountId) {
      params = params.set(
        'accountId',
        filter.accountId
      );
    }

    if (filter.type) {
      params = params.set(
        'type',
        filter.type
      );
    }

    return this.http.get<Transaction[]>(
      `${this.apiUrl}/filter`,
      { params }
    );
  }
}