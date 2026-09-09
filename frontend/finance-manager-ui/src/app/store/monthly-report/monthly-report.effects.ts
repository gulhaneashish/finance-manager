import { Injectable, inject } from '@angular/core';

import {
  Actions,
  createEffect,
  ofType
} from '@ngrx/effects';

import {
  catchError,
  map,
  mergeMap,
  of
} from 'rxjs';

import * as MonthlyReportActions
  from './monthly-report.actions';

import { MonthlyReportService }
  from '../../core/services/monthly-report.service';

@Injectable()
export class MonthlyReportEffects {

  private actions$ = inject(Actions);

  private monthlyReportService =
    inject(MonthlyReportService);

  loadReport$ = createEffect(() =>
    this.actions$.pipe(

      ofType(
        MonthlyReportActions.loadMonthlyReport
      ),

      mergeMap(({ year, month }) =>

        this.monthlyReportService
          .getReport(year, month)
          .pipe(

            map(report =>
              MonthlyReportActions.loadMonthlyReportSuccess({
                report
              })
            ),

            catchError(error =>
              of(
                MonthlyReportActions.loadMonthlyReportFailure({
                  error:
                    error?.error?.message ??
                    'Failed to load monthly report.'
                })
              )
            )

          )

      )

    )
  );

}