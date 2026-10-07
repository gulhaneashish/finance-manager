import { Component, HostListener, OnInit, inject } from '@angular/core';
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
  FormsModule,
  ReactiveFormsModule
} from '@angular/forms';

import { Transaction, TransactionType } from '../../../core/models/transaction.model';
import { Category } from '../../../core/models/category.model';

import {
  selectAllCategories
} from '../../../store/categories/category.selectors';

import {
  loadCategories
} from '../../../store/categories/category.actions';
import { TransactionFilter } from '../../../core/models/transaction-filter.model';

import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [
    AsyncPipe,
    DatePipe,
    DecimalPipe,
    MatIconModule,
    TransactionForm,
    TransferForm,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './transaction-list.html',
  styleUrl: './transaction-list.css'
})
export class TransactionList implements OnInit {

  TransactionType = TransactionType;
  showTransactionForm = false;
  showTransferForm = false;
  showFilters = false;

  // Sorting State - default to 'newest' (order as entered, newest at top)
  sortBy: 'newest' | 'oldest' | 'dateDesc' | 'dateAsc' | 'amountDesc' | 'amountAsc' = 'newest';

  // Pagination State
  currentPage = 1;
  pageSize = 10;
  pageSizeOptions = [5, 10, 20, 50];

  private store = inject(Store);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

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

  this.currentPage = 1;

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

@HostListener('document:keydown.escape')
onEscape(): void {
  if (this.showTransactionForm) {
    this.closeTransactionForm();
  }
  if (this.showTransferForm) {
    this.closeTransferForm();
  }
}

// ==========================================
// SORTING HELPERS
// ==========================================
onSortChange(): void {
  this.currentPage = 1;
}

toggleSort(column: 'id' | 'date' | 'amount'): void {
  if (column === 'id') {
    this.sortBy = this.sortBy === 'newest' ? 'oldest' : 'newest';
  } else if (column === 'date') {
    this.sortBy = this.sortBy === 'dateDesc' ? 'dateAsc' : 'dateDesc';
  } else if (column === 'amount') {
    this.sortBy = this.sortBy === 'amountDesc' ? 'amountAsc' : 'amountDesc';
  }
  this.currentPage = 1;
}

getSortedTransactions(transactions: Transaction[]): Transaction[] {
  if (!transactions || transactions.length === 0) return [];
  const copy = [...transactions];
  switch (this.sortBy) {
    case 'newest': // Default: Most recently entered first (numeric Id descending)
      return copy.sort((a, b) => b.id - a.id);
    case 'oldest': // First entered first (numeric Id ascending)
      return copy.sort((a, b) => a.id - b.id);
    case 'dateDesc': // Transaction date newest first, ties broken by Id desc
      return copy.sort((a, b) => {
        const timeDiff = new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime();
        return timeDiff !== 0 ? timeDiff : b.id - a.id;
      });
    case 'dateAsc': // Transaction date oldest first, ties broken by Id asc
      return copy.sort((a, b) => {
        const timeDiff = new Date(a.transactionDate).getTime() - new Date(b.transactionDate).getTime();
        return timeDiff !== 0 ? timeDiff : a.id - b.id;
      });
    case 'amountDesc':
      return copy.sort((a, b) => (b.amount - a.amount) || (b.id - a.id));
    case 'amountAsc':
      return copy.sort((a, b) => (a.amount - b.amount) || (b.id - a.id));
    default:
      return copy.sort((a, b) => b.id - a.id);
  }
}

// ==========================================
// PAGINATION HELPERS
// ==========================================
getPagedTransactions(transactions: Transaction[]): Transaction[] {
  const sorted = this.getSortedTransactions(transactions);
  const totalPages = this.getTotalPages(sorted.length);
  if (this.currentPage > totalPages && totalPages > 0) {
    this.currentPage = totalPages;
  }
  const startIndex = (this.currentPage - 1) * this.pageSize;
  return sorted.slice(startIndex, startIndex + this.pageSize);
}

getTotalPages(totalItems: number): number {
  return Math.max(1, Math.ceil(totalItems / this.pageSize));
}

getStartIndex(totalItems: number): number {
  if (totalItems === 0) return 0;
  return (this.currentPage - 1) * this.pageSize + 1;
}

getEndIndex(totalItems: number): number {
  return Math.min(this.currentPage * this.pageSize, totalItems);
}

setPage(page: number, totalItems: number): void {
  const maxPage = this.getTotalPages(totalItems);
  if (page < 1) page = 1;
  if (page > maxPage) page = maxPage;
  this.currentPage = page;
}

onPageSizeChange(newSize: any): void {
  this.pageSize = Number(newSize);
  this.currentPage = 1;
}

getPageNumbers(totalItems: number): number[] {
  const totalPages = this.getTotalPages(totalItems);
  const current = this.currentPage;
  const maxVisiblePages = 5;

  if (totalPages <= maxVisiblePages) {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  let start = Math.max(1, current - 2);
  let end = Math.min(totalPages, start + maxVisiblePages - 1);

  if (end - start + 1 < maxVisiblePages) {
    start = Math.max(1, end - maxVisiblePages + 1);
  }

  const pages: number[] = [];
  for (let i = start; i <= end; i++) {
    pages.push(i);
  }
  return pages;
}
}