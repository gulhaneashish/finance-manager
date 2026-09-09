import { createFeatureSelector, createSelector } from '@ngrx/store';

import { InvestmentState } from './investment.reducer';

export const selectInvestmentState =
  createFeatureSelector<InvestmentState>('investment');

export const selectInvestments = createSelector(
  selectInvestmentState,
  (state) => state.investments
);

export const selectInvestmentSummary = createSelector(
  selectInvestmentState,
  (state) => state.summary
);

export const selectInvestmentLoading = createSelector(
  selectInvestmentState,
  (state) => state.loading
);

export const selectInvestmentError = createSelector(
  selectInvestmentState,
  (state) => state.error
);

export const selectInvestmentOperationSuccess = createSelector(
  selectInvestmentState,
  (state) => state.operationSuccess
);