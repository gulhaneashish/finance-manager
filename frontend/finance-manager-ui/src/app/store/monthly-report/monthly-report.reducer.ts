import { createReducer, on } from '@ngrx/store';

import {
  loadMonthlyReport,
  loadMonthlyReportSuccess,
  loadMonthlyReportFailure
} from './monthly-report.actions';

import { MonthlyReport } from '../../core/models/monthly-report.model';

export interface MonthlyReportState {
  report: MonthlyReport | null;
  loading: boolean;
  error: string | null;
}

export const initialState: MonthlyReportState = {
  report: null,
  loading: false,
  error: null
};

export const monthlyReportReducer = createReducer(

  initialState,

  on(
    loadMonthlyReport,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    loadMonthlyReportSuccess,
    (state, { report }) => ({
      ...state,
      report,
      loading: false,
      error: null
    })
  ),

  on(
    loadMonthlyReportFailure,
    (state, { error }) => ({
      ...state,
      report: null,
      loading: false,
      error
    })
  )

);