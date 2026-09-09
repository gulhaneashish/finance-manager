import { Component, OnInit, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';

import {
  deleteTransaction,
  loadTransactions,
  filterTransactions
} from '../../../store/transactions/transaction.actions';

import {
  selectAllTransactions,
  selectTransactionsLoading,
  selectTransactionsError
} from '../../../store/transactions/transaction.selectors';

import { TransactionForm } from '../transaction-form/transaction-form';
import { TransferForm } from '../transfer-form/transfer-form';

import { selectAccounts } from '../../../store/accounts/accounts.selectors';
import { loadAccounts } from '../../../store/accounts/accounts.actions';

import {
  FormBuilder,
  ReactiveFormsModule
} from '@angular/forms';

import { TransactionType } from   '../../../core/models/transaction.model';
import { Category } from '../../../core/models/category.model';

import {
  selectAllCategories
} from '../../../store/categories/category.selectors';

import {
  loadCategories
} from '../../../store/categories/category.actions';
import { TransactionFilter } from '../../../core/models/transaction-filter.model';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [
    AsyncPipe,
    DatePipe,
    DecimalPipe,
    TransactionForm,
    TransferForm,
    ReactiveFormsModule
  ],
  templateUrl: './transaction-list.html',
  styleUrl: './transaction-list.css'
})
export class TransactionList implements OnInit {

  TransactionType=TransactionType;
showTransactionForm = false;

showTransferForm = false;

showFilters = false;
  private store = inject(Store);
  private fb = inject(FormBuilder);
  
categories$ =
  this.store.select(selectAllCategories);
  transactions$ = this.store.select(
    selectAllTransactions
  );

  accounts$ = this.store.select(
    selectAccounts
  );

  loading$ = this.store.select(
    selectTransactionsLoading
  );

  error$ = this.store.select(
    selectTransactionsError
  );

  filterForm = this.fb.group({
    fromDate: [''],
    toDate: [''],
    accountId: [0],
    type: [null as string | null]
  });

  ngOnInit(): void {

    this.store.dispatch(
      loadTransactions()
    );
    this.store.dispatch(loadCategories());
    this.store.dispatch(
      loadAccounts()
    );
  }

  deleteTransaction(id: number): void {

    const confirmed = confirm(
      'Are you sure you want to delete this transaction?'
    );

    if (!confirmed) {
      return;
    }

    this.store.dispatch(
      deleteTransaction({ id })
    );
  }

  applyFilters(): void {

  const value = this.filterForm.getRawValue();

  const filter: TransactionFilter = {

    fromDate:
      value.fromDate || undefined,

    toDate:
      value.toDate || undefined,

    accountId:
      value.accountId &&
      value.accountId > 0
        ? value.accountId
        : undefined,

    type:
      value.type !== null
        ? value.type
        : undefined
  };

  this.store.dispatch(
    filterTransactions({ filter })
  );
}

openTransactionForm(): void {
  this.showTransactionForm = true;

  this.showTransferForm = false;
}

closeTransactionForm(): void {
  this.showTransactionForm = false;
}


openTransferForm(): void {
  this.showTransferForm = true;

  this.showTransactionForm = false;
}

closeTransferForm(): void {
  this.showTransferForm = false;
}


toggleFilters(): void {
  this.showFilters = !this.showFilters;
}
}