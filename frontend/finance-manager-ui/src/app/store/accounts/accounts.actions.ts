import { createAction, props } from '@ngrx/store';

import { Account } from '../../core/models/account.model';

export const loadAccounts = createAction(
  '[Accounts] Load Accounts'
);


export const loadAccountsSuccess = createAction(
  '[Accounts API] Load Accounts Success',
  props<{
    accounts: Account[];
  }>()
);

export const loadAccountsFailure = createAction(
  '[Accounts API] Load Accounts Failure',
  props<{
    error: string;
  }>()
);

export const loadActiveAccounts = createAction(
  '[Accounts] Load Active Accounts'
);
export const loadActiveAccountsSuccess = createAction(
  '[Accounts API] Load Active Accounts Success',
  props<{
    accounts: Account[];
  }>()
);
export const loadActiveAccountsFailure = createAction(
  '[Accounts API] Load Active Accounts Failure',
  props<{
    error: string;
  }>()
);

export const createAccount = createAction(
  '[Accounts] Create Account',
  props<{
    account: {
      name: string;
      accountType: string;
      openingBalance: number;
      creditLimit?: number | null;
    };
  }>()
);

export const createAccountSuccess = createAction(
  '[Accounts API] Create Account Success',
  props<{
    account: Account;
  }>()
);

export const createAccountFailure = createAction(
  '[Accounts API] Create Account Failure',
  props<{
    error: string;
  }>()
);

export const updateAccount = createAction(
  '[Accounts] Update Account',
  props<{
    id: number;
    account: {
      name: string;
      accountType: string;
      openingBalance: number;
      creditLimit?: number | null;
      isActive: boolean;
    };
  }>()
);

export const updateAccountSuccess = createAction(
  '[Accounts API] Update Account Success',
  props<{
    account: Account;
  }>()
);

export const updateAccountFailure = createAction(
  '[Accounts API] Update Account Failure',
  props<{
    error: string;
  }>()
);

export const deleteAccount = createAction(
  '[Accounts] Delete Account',
  props<{ id: number }>()
);

export const deleteAccountSuccess = createAction(
  '[Accounts API] Delete Account Success',
  props<{ id: number }>()
);

export const deleteAccountFailure = createAction(
  '[Accounts API] Delete Account Failure',
  props<{ error: string }>()
);



