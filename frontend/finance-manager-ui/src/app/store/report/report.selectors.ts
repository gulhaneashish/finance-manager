import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ReportState } from './report.reducer';

export const selectReportState =
  createFeatureSelector<ReportState>('report');

export const selectReport =
  createSelector(
    selectReportState,
    state => state.report
  );

export const selectTotalIncome =
  createSelector(
    selectReport,
    report => report?.totalIncome ?? 0
  );

export const selectTotalExpense =
  createSelector(
    selectReport,
    report => report?.totalExpense ?? 0
  );

export const selectNetAmount =
  createSelector(
    selectReport,
    report => report?.netAmount ?? 0
  );

export const selectCategoryExpenses =
  createSelector(
    selectReport,
    report => report?.categoryExpenses ?? []
  );

export const selectReportLoading =
  createSelector(
    selectReportState,
    state => state.loading
  );

export const selectReportError =
  createSelector(
    selectReportState,
    state => state.error
  );