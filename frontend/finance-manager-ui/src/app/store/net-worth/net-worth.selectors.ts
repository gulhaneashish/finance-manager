import {
  createFeatureSelector,
  createSelector
} from '@ngrx/store';

import { NetWorthState } from './net-worth.reducer';

export const selectNetWorthState =
  createFeatureSelector<NetWorthState>(
    'netWorth'
  );

export const selectNetWorth =
  createSelector(
    selectNetWorthState,
    state => state.netWorth
  );

export const selectNetWorthLoading =
  createSelector(
    selectNetWorthState,
    state => state.loading
  );

export const selectNetWorthError =
  createSelector(
    selectNetWorthState,
    state => state.error
  );

export const selectTotalAssets =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.totalAssets ?? 0
  );

export const selectBankBalance =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.bankBalance ?? 0
  );

export const selectCashBalance =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.cashBalance ?? 0
  );

export const selectSavingsBalance =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.savingsBalance ?? 0
  );

export const selectInvestmentBalance =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.investmentBalance ?? 0
  );

export const selectCreditCardDebt =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.creditCardDebt ?? 0
  );

export const selectLoansPayable =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.loansPayable ?? 0
  );

export const selectLoansReceivable =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.loansReceivable ?? 0
  );

export const selectTotalLiabilities =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.totalLiabilities ?? 0
  );

export const selectNetWorthAmount =
  createSelector(
    selectNetWorth,
    netWorth => netWorth?.netWorth ?? 0
  );