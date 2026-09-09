import {
  createFeatureSelector,
  createSelector
} from '@ngrx/store';

import {
  DashboardState
} from './dashboard.reducer';


export const selectDashboardState =
  createFeatureSelector<DashboardState>(
    'dashboard'
  );


// ===============================
// Summary
// ===============================

export const selectDashboard =
  createSelector(
    selectDashboardState,
    state => state.data
  );


export const selectDashboardLoading =
  createSelector(
    selectDashboardState,
    state => state.loading
  );


export const selectDashboardError =
  createSelector(
    selectDashboardState,
    state => state.error
  );


// ===============================
// Category Spending
// ===============================

export const selectCategorySpending =
  createSelector(
    selectDashboardState,
    state => state.categorySpending
  );


export const selectCategorySpendingLoading =
  createSelector(
    selectDashboardState,
    state => state.categorySpendingLoading
  );


export const selectCategorySpendingError =
  createSelector(
    selectDashboardState,
    state => state.categorySpendingError
  );

  export const selectMonthlyCashFlow =
  createSelector(
    selectDashboardState,
    state => state.monthlyCashFlow
  );

export const selectMonthlyCashFlowLoading =
  createSelector(
    selectDashboardState,
    state => state.monthlyCashFlowLoading
  );

export const selectMonthlyCashFlowError =
  createSelector(
    selectDashboardState,
    state => state.monthlyCashFlowError
  );
  export const selectLoanDebt =
  createSelector(
    selectDashboardState,
    state => state.loanDebt
  );
  export const selectAccounts =
  createSelector(
    selectDashboardState,
    state => state.accounts
  );
  export const selectSavingsInvestments =
  createSelector(
    selectDashboardState,
    state => state.savingsInvestments
  );