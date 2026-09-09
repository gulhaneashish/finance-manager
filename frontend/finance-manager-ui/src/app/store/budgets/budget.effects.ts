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

import * as BudgetActions
  from './budget.actions';

import { BudgetService }
  from '../../core/services/budget.service';
import { loadBudgetFailure, loadBudgetSuccess } from './budget.actions';

@Injectable()
export class BudgetEffects {

  private actions$ = inject(Actions);

  private budgetService =
    inject(BudgetService);

  loadBudget$ = createEffect(() =>
    this.actions$.pipe(

      ofType(BudgetActions.loadBudget),

      switchMap(({ year, month }) =>
        this.budgetService
          .get(year, month)
          .pipe(

            map(budget =>
              BudgetActions.loadBudgetSuccess({
                budget
              })
            ),

            catchError(error =>
              of(
                BudgetActions.loadBudgetFailure({
                  error:
                    error.error?.message ||
                    'Budget not found.'
                })
              )
            )

          )
      )
    )
  );

  createBudget$ = createEffect(() =>
    this.actions$.pipe(

      ofType(BudgetActions.createBudget),

      switchMap(({ budget }) =>
        this.budgetService
          .create(budget)
          .pipe(

            map(response =>
              BudgetActions.createBudgetSuccess({
                message: response.message
              })
            ),

            catchError(error =>
              of(
                BudgetActions.createBudgetFailure({
                  error:
                    error.error?.message ||
                    'Failed to create budget.'
                })
              )
            )

          )
      )
    )
  );
updateBudget$ = createEffect(() =>
  this.actions$.pipe(

    ofType(BudgetActions.updateBudget),

    switchMap(({ year, month, budget }) =>
      this.budgetService
        .update(year, month, budget)
        .pipe(

        map(response =>
  BudgetActions.updateBudgetSuccess({
    message: response.message,
    year,
    month
  })
),

          catchError(error =>
            of(
              BudgetActions.updateBudgetFailure({
                error:
                  error.error?.message ||
                  'Failed to update budget.'
              })
            )
          )

        )
    )
  )
);

reloadAfterUpdate$ = createEffect(() =>
  this.actions$.pipe(
    ofType(BudgetActions.updateBudgetSuccess),

    map(({ year, month }) =>
      BudgetActions.loadBudget({
        year,
        month
      })
    )
  )
);
}