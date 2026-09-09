import {
  createFeatureSelector,
  createSelector
} from '@ngrx/store';

import { LoanState } from './loan.reducer';

export const selectLoanState =
  createFeatureSelector<LoanState>('loans');

export const selectLoans =
  createSelector(
    selectLoanState,
    state => state.loans
  );

export const selectLoanLoading =
  createSelector(
    selectLoanState,
    state => state.loading
  );

export const selectLoanError =
  createSelector(
    selectLoanState,
    state => state.error
  );

  export const selectLoanSuccessMessage =
  createSelector(
    selectLoanState,
    state => state.successMessage
  );