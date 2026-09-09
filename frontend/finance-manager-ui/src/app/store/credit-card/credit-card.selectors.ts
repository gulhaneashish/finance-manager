import { createFeatureSelector, createSelector } from '@ngrx/store';

import { CreditCardState } from './credit-card.reducer';

export const selectCreditCardState =
  createFeatureSelector<CreditCardState>('creditCard');

export const selectCreditCardLoading =
  createSelector(
    selectCreditCardState,
    state => state.loading
  );

export const selectCreditCardError =
  createSelector(
    selectCreditCardState,
    state => state.error
  );

export const selectCreditCardSuccessMessage =
  createSelector(
    selectCreditCardState,
    state => state.successMessage
  );