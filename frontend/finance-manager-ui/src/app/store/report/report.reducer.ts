import { createReducer, on } from '@ngrx/store';

import {
  loadReport,
  loadReportSuccess,
  loadReportFailure
} from './report.actions';

import { ReportSummary } from '../../core/models/report.model';

export interface ReportState {
  report: ReportSummary | null;
  loading: boolean;
  error: string | null;
}

export const initialState: ReportState = {
  report: null,
  loading: false,
  error: null
};

export const reportReducer = createReducer(

  initialState,

  on(
    loadReport,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    loadReportSuccess,
    (state, { report }) => ({
      ...state,
      report,
      loading: false,
      error: null
    })
  ),

  on(
    loadReportFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  )

);