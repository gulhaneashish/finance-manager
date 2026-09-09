import { Injectable, inject } from '@angular/core';

import {
  Actions,
  createEffect,
  ofType
} from '@ngrx/effects';

import {
  catchError,
  map,
  of,
  switchMap
} from 'rxjs';

import * as ReportActions from './report.actions';
import { ReportService } from '../../core/services/report.service';

@Injectable()
export class ReportEffects {

  private actions$ = inject(Actions);

  private reportService = inject(ReportService);

  loadReport$ = createEffect(() =>
    this.actions$.pipe(

      ofType(ReportActions.loadReport),

      switchMap(({ fromDate, toDate }) =>

        this.reportService
          .getSummary(fromDate, toDate)
          .pipe(

            map(report =>
              ReportActions.loadReportSuccess({
                report
              })
            ),

            catchError(error =>
              of(
                ReportActions.loadReportFailure({
                  error:
                    error.error?.message ||
                    'Failed to load report.'
                })
              )
            )

          )

      )

    )
  );

}