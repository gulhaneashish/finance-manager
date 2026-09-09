import { createReducer, on } from '@ngrx/store';

import * as TransactionActions
  from './transaction.actions';

import { Transaction } from '../../core/models/transaction.model';

export interface TransactionsState {
  transactions: Transaction[];
  loading: boolean;
  creating: boolean;
  deleting: boolean;
  error: string | null;
}

export const initialState: TransactionsState = {
  transactions: [],
  loading: false,
  creating: false,
  deleting: false,
  error: null
};

export const transactionReducer = createReducer(

  initialState,

  on(
    TransactionActions.loadTransactions,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    TransactionActions.loadTransactionsSuccess,
    (state, { transactions }) => ({
      ...state,
      transactions,
      loading: false,
      error: null
    })
  ),

  on(
    TransactionActions.loadTransactionsFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

  on(
    TransactionActions.createTransaction,
    state => ({
      ...state,
      creating: true,
      error: null
    })
  ),

  on(
    TransactionActions.createTransactionSuccess,
    (state, { transaction }) => ({
      ...state,
      transactions: [
        transaction,
        ...state.transactions
      ],
      creating: false,
      error: null
    })
  ),

  on(
    TransactionActions.createTransactionFailure,
    (state, { error }) => ({
      ...state,
      creating: false,
      error
    })
  ),

  on(
    TransactionActions.deleteTransaction,
    state => ({
      ...state,
      deleting: true,
      error: null
    })
  ),

  on(
    TransactionActions.deleteTransactionSuccess,
    (state, { id }) => ({
      ...state,
      transactions:
        state.transactions.filter(
          transaction =>
            transaction.id !== id
        ),
      deleting: false,
      error: null
    })
  ),

  on(
    TransactionActions.deleteTransactionFailure,
    (state, { error }) => ({
      ...state,
      deleting: false,
      error
    })
  ),

  on(
  TransactionActions.filterTransactions,
  state => ({
    ...state,
    loading: true,
    error: null
  })
),

on(
  TransactionActions.filterTransactionsSuccess,
  (state, { transactions }) => ({
    ...state,
    transactions,
    loading: false,
    error: null
  })
),

on(
  TransactionActions.filterTransactionsFailure,
  (state, { error }) => ({
    ...state,
    loading: false,
    error
  })
),

);