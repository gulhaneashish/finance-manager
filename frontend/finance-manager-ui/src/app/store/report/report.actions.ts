import { createAction, props } from '@ngrx/store';

import {
  ReportSummary
} from '../../core/models/report.model';

export const loadReport = createAction(
  '[Report] Load Report',
  props<{
    fromDate?: string;
    toDate?: string;
  }>()
);

export const loadReportSuccess = createAction(
  '[Report] Load Report Success',
  props<{
    report: ReportSummary;
  }>()
);

export const loadReportFailure = createAction(
  '[Report] Load Report Failure',
  props<{
    error: string;
  }>()
);