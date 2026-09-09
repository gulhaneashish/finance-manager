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

import * as DashboardActions
  from './dashboard.actions';

import { DashboardService }
  from '../../core/services/dashboard';

@Injectable()
export class DashboardEffects {

  private actions$ = inject(Actions);

  private dashboardService =
    inject(DashboardService);

  loadDashboard$ = createEffect(() =>
    this.actions$.pipe(

      ofType(
        DashboardActions.loadDashboard
      ),

      switchMap(({ year, month }) =>
        this.dashboardService
          .getSummary(year, month)
          .pipe(

            map(dashboard =>
              DashboardActions.loadDashboardSuccess({
                dashboard
              })
            ),

            catchError(error =>
              of(
                DashboardActions.loadDashboardFailure({
                  error:
                    error.error?.detail ??
                    'Failed to load dashboard.'
                })
              )
            )
          )
      )
    )
  );
  loadCategorySpending$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      DashboardActions.loadCategorySpending
    ),

    switchMap(({ year, month }) =>

      this.dashboardService
        .getCategorySpending(year, month)

        .pipe(

          map(categorySpending =>
            DashboardActions.loadCategorySpendingSuccess({
              categorySpending
            })
          ),

          catchError(error =>
            of(
              DashboardActions.loadCategorySpendingFailure({
                error:
                  error.error?.detail ??
                  'Failed to load category spending.'
              })
            )
          )

        )
    )
  )
);

loadMonthlyCashFlow$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      DashboardActions.loadMonthlyCashFlow
    ),

    switchMap(({ year }) =>

      this.dashboardService
        .getMonthlyCashFlow(year)
        .pipe(

          map(monthlyCashFlow =>
            DashboardActions.loadMonthlyCashFlowSuccess({
              monthlyCashFlow
            })
          ),

          catchError(error =>
            of(
              DashboardActions.loadMonthlyCashFlowFailure({
                error:
                  error.error?.detail ??
                  'Failed to load monthly cash flow.'
              })
            )
          )

        )
    )
  )
);

loadLoanDebt$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      DashboardActions.loadLoanDebt
    ),

    switchMap(() =>
      this.dashboardService
        .getLoanDebt()
        .pipe(

          map(loanDebt =>
            DashboardActions.loadLoanDebtSuccess({
              loanDebt
            })
          ),

          catchError(error =>
            of(
              DashboardActions.loadLoanDebtFailure({
                error:
                  error.error?.detail ??
                  'Failed to load loan debt.'
              })
            )
          )

        )
    )
  )
);
loadAccounts$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      DashboardActions.loadAccounts
    ),

    switchMap(() =>
      this.dashboardService
        .getAccounts()
        .pipe(

          map(accounts =>
            DashboardActions.loadAccountsSuccess({
              accounts
            })
          ),

          catchError(error =>
            of(
              DashboardActions.loadAccountsFailure({
                error:
                  error.error?.detail ??
                  'Failed to load accounts.'
              })
            )
          )

        )
    )

  )
);
loadSavingsInvestments$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      DashboardActions.loadSavingsInvestments
    ),

    switchMap(({ year, month }) =>
      this.dashboardService
        .getSavingsInvestments(year, month)
        .pipe(

          map(savingsInvestments =>
            DashboardActions
              .loadSavingsInvestmentsSuccess({
                savingsInvestments
              })
          ),

          catchError(error =>
            of(
              DashboardActions
                .loadSavingsInvestmentsFailure({
                  error:
                    error.error?.detail ??
                    'Failed to load savings and investments.'
                })
            )
          )

        )
    )

  )
);
}