import {
  createFeatureSelector,
  createSelector
} from '@ngrx/store';

import { MonthlyReportState } from './monthly-report.reducer';

export const selectMonthlyReportState =
  createFeatureSelector<MonthlyReportState>(
    'monthlyReport'
  );

export const selectMonthlyReport =
  createSelector(
    selectMonthlyReportState,
    state => state.report
  );

export const selectMonthlyReportLoading =
  createSelector(
    selectMonthlyReportState,
    state => state.loading
  );

export const selectMonthlyReportError =
  createSelector(
    selectMonthlyReportState,
    state => state.error
  );

export const selectMonthlyReportYear =
  createSelector(
    selectMonthlyReport,
    report => report?.year ?? null
  );

export const selectMonthlyReportMonth =
  createSelector(
    selectMonthlyReport,
    report => report?.month ?? null
  );

export const selectMonthlyReportIncome =
  createSelector(
    selectMonthlyReport,
    report => report?.totalIncome ?? 0
  );

export const selectMonthlyReportExpenses =
  createSelector(
    selectMonthlyReport,
    report => report?.totalExpenses ?? 0
  );

export const selectMonthlyReportNetCashFlow =
  createSelector(
    selectMonthlyReport,
    report => report?.netCashFlow ?? 0
  );

export const selectMonthlyReportExpenseBudget =
  createSelector(
    selectMonthlyReport,
    report => report?.expenseBudget ?? 0
  );

export const selectMonthlyReportBudgetSpent =
  createSelector(
    selectMonthlyReport,
    report => report?.budgetSpent ?? 0
  );

export const selectMonthlyReportBudgetRemaining =
  createSelector(
    selectMonthlyReport,
    report => report?.budgetRemaining ?? 0
  );

export const selectMonthlyReportSavingsTarget =
  createSelector(
    selectMonthlyReport,
    report => report?.savingsTarget ?? 0
  );

export const selectMonthlyReportInvestmentTarget =
  createSelector(
    selectMonthlyReport,
    report => report?.investmentTarget ?? 0
  );

export const selectMonthlyReportSavingsAmount =
  createSelector(
    selectMonthlyReport,
    report => report?.savingsAmount ?? 0
  );

export const selectMonthlyReportInvestmentAmount =
  createSelector(
    selectMonthlyReport,
    report => report?.investmentAmount ?? 0
  );

export const selectMonthlyReportTotalBorrowed =
  createSelector(
    selectMonthlyReport,
    report => report?.totalBorrowed ?? 0
  );

export const selectMonthlyReportTotalLent =
  createSelector(
    selectMonthlyReport,
    report => report?.totalLent ?? 0
  );

export const selectMonthlyReportCategories =
  createSelector(
    selectMonthlyReport,
    report => report?.categories ?? []
  );