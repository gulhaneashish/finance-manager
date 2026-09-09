import {
  createFeatureSelector,
  createSelector
} from '@ngrx/store';

import {
  AccountsState
} from './accounts.reducer';

export const selectAccountsState =
  createFeatureSelector<AccountsState>(
    'accounts'
  );

export const selectAccounts =
  createSelector(
    selectAccountsState,
    state => state.accounts
  );
export const selectActiveAccounts =
  createSelector(
    selectAccountsState,
    state => state.activeAccounts
  );
export const selectAccountsLoading =
  createSelector(
    selectAccountsState,
    state => state.loading
  );

export const selectAccountsError =
  createSelector(
    selectAccountsState,
    state => state.error
  );

  export const selectAccountsCreating =
  createSelector(
    selectAccountsState,
    state => state.creating
  );