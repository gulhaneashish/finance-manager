import { createAction, props } from '@ngrx/store';
import { Transaction, TransactionPurpose, TransactionType } from '../../core/models/transaction.model';
import { TransactionFilter } from '../../core/models/transaction-filter.model';

export const loadTransactions = createAction(
  '[Transactions] Load Transactions'
);

export const loadTransactionsSuccess = createAction(
  '[Transactions API] Load Transactions Success',
  props<{ transactions: Transaction[] }>()
);

export const loadTransactionsFailure = createAction(
  '[Transactions API] Load Transactions Failure',
  props<{ error: string }>()
);

export const createTransaction = createAction(
  '[Transactions] Create Transaction',
  props<{
    transaction: {
      accountId: number | null;
      fromAccountId?: number | null;
      toAccountId?: number | null;
      categoryId?: number | null;
      amount: number;
      type: TransactionType;
      description: string;
      transactionDate: string;
      purpose: TransactionPurpose;
    };
  }>()
);

export const createTransactionSuccess = createAction(
  '[Transactions API] Create Transaction Success',
  props<{ transaction: Transaction }>()
);

export const createTransactionFailure = createAction(
  '[Transactions API] Create Transaction Failure',
  props<{ error: string }>()
);

export const deleteTransaction = createAction(
  '[Transactions] Delete Transaction',
  props<{ id: number }>()
);

export const deleteTransactionSuccess = createAction(
  '[Transactions API] Delete Transaction Success',
  props<{ id: number }>()
);

export const deleteTransactionFailure = createAction(
  '[Transactions API] Delete Transaction Failure',
  props<{ error: string }>()
);

export const createTransfer = createAction(
  '[Transactions] Create Transfer',
  props<{
    transfer: {
      fromAccountId: number;
      toAccountId: number;
      amount: number;
      transactionPurpose: TransactionPurpose;
      transactionDate: string;
      description?: string;
    };
  }>()
);

export const createTransferSuccess = createAction(
  '[Transactions API] Create Transfer Success'
);

export const createTransferFailure = createAction(
  '[Transactions API] Create Transfer Failure',
  props<{ error: string }>()
);

export const filterTransactions = createAction(
  '[Transactions] Filter Transactions',
  props<{ filter: TransactionFilter }>()
);

export const filterTransactionsSuccess = createAction(
  '[Transactions API] Filter Transactions Success',
  props<{ transactions: Transaction[] }>()
);

export const filterTransactionsFailure = createAction(
  '[Transactions API] Filter Transactions Failure',
  props<{ error: string }>()
);