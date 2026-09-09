import { createAction, props } from '@ngrx/store';

import { MonthlyReport } from '../../core/models/monthly-report.model';

export const loadMonthlyReport = createAction(
  '[Monthly Report] Load Monthly Report',
  props<{
    year: number;
    month: number;
  }>()
);

export const loadMonthlyReportSuccess = createAction(
  '[Monthly Report API] Load Monthly Report Success',
  props<{
    report: MonthlyReport;
  }>()
);

export const loadMonthlyReportFailure = createAction(
  '[Monthly Report API] Load Monthly Report Failure',
  props<{
    error: string;
  }>()
);