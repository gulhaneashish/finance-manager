import { createReducer, on } from '@ngrx/store';

import {
  loadAccounts,
  loadAccountsSuccess,
  loadAccountsFailure,
  createAccountSuccess,
  createAccountFailure,
  createAccount,
  updateAccountFailure,
  updateAccountSuccess,
  updateAccount,
  deleteAccountSuccess,
  loadActiveAccounts,
  loadActiveAccountsSuccess,
  loadActiveAccountsFailure
} from './accounts.actions';

import { Account } from '../../core/models/account.model';

export interface AccountsState {
  accounts: Account[];
  activeAccounts: Account[];
  loading: boolean;
  creating: boolean;
  error: string | null;
}

export const initialAccountsState: AccountsState = {
  accounts: [],
  activeAccounts: [],
  loading: false,
  creating: false,
  error: null
};

export const accountsReducer = createReducer(

  initialAccountsState,

  on(loadAccounts, state => ({
    ...state,
    loading: true,
    error: null
  })),

  on(
    loadAccountsSuccess,
    (state, { accounts }) => ({
      ...state,
      accounts,
      loading: false,
      error: null
    })
  ),

  on(
    loadAccountsFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

   on(loadActiveAccounts, state => ({
    ...state,
    loading: true,
    error: null
  })),

  on(
  loadActiveAccountsSuccess,
  (state, { accounts }) => ({
    ...state,
    activeAccounts: accounts,
    loading: false,
    error: null
  })
),

  on(
    loadActiveAccountsFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),
  on(
  createAccount,
  state => ({
    ...state,
    creating: true,
    error: null
  })
),

on(
  createAccountSuccess,
  (state, { account }) => ({
    ...state,
    accounts: [
      ...state.accounts,
      account
    ],
    creating: false,
    error: null
  })
),

on(
  createAccountFailure,
  (state, { error }) => ({
    ...state,
    creating: false,
    error
  })
),

on(
  updateAccount,
  state => ({
    ...state,
    creating: true,
    error: null
  })
),

on(
  updateAccountSuccess,
  (state, { account }) => ({
    ...state,
    accounts: state.accounts.map(
      existing =>
        existing.id === account.id
          ? account
          : existing
    ),
    creating: false,
    error: null
  })
),

on(
  updateAccountFailure,
  (state, { error }) => ({
    ...state,
    creating: false,
    error
  })
),

on(
  deleteAccountSuccess,
  (state, { id }) => ({
    ...state,

    accounts: state.accounts.filter(
      account => account.id !== id
    ),

    error: null
  })
),
);