import { Injectable, inject } from '@angular/core';

import {
  Actions,
  createEffect,
  ofType
} from '@ngrx/effects';
import * as NetWorthActions
  from '../net-worth/net-worth.actions';
import {
  catchError,
  map,
  mergeMap,
  of
} from 'rxjs';
import { Router } from '@angular/router';
import { InvestmentService } from '../../core/services/investment.service';

import * as InvestmentActions
  from './investment.actions';

@Injectable()
export class InvestmentEffects {

  private actions$ = inject(Actions);
  private investmentService = inject(InvestmentService);
private router = inject(Router);

navigateAfterCreate$ = createEffect(
  () =>
    this.actions$.pipe(
      ofType(
        InvestmentActions.createInvestmentSuccess
      ),

      map(() => {
        this.router.navigate(['/investments']);
      })
    ),
  { dispatch: false }
);
  loadInvestments$ = createEffect(() =>
    this.actions$.pipe(
      ofType(InvestmentActions.loadInvestments),

      mergeMap(() =>
        this.investmentService
          .getInvestments()
          .pipe(

            map(investments =>
              InvestmentActions.loadInvestmentsSuccess({
                investments
              })
            ),

            catchError(error =>
              of(
                InvestmentActions.loadInvestmentsFailure({
                  error: this.getErrorMessage(error)
                })
              )
            )

          )
      )
    )
  );


  loadInvestmentSummary$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        InvestmentActions.loadInvestmentSummary
      ),

      mergeMap(() =>
        this.investmentService
          .getSummary()
          .pipe(

            map(summary =>
              InvestmentActions.loadInvestmentSummarySuccess({
                summary
              })
            ),

            catchError(error =>
              of(
                InvestmentActions.loadInvestmentSummaryFailure({
                  error: this.getErrorMessage(error)
                })
              )
            )

          )
      )
    )
  );


  createInvestment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        InvestmentActions.createInvestment
      ),

      mergeMap(action =>
        this.investmentService
          .createInvestment({
            accountId: action.accountId,
            name: action.name,
            investmentType: action.investmentType,
            amount: action.amount,
            investmentDate: action.investmentDate,
            description: action.description
          })
          .pipe(

            map(response =>
              InvestmentActions.createInvestmentSuccess({
                message:
                  response?.message ??
                  'Investment created successfully.'
              })
            ),

            catchError(error =>
              of(
                InvestmentActions.createInvestmentFailure({
                  error: this.getErrorMessage(error)
                })
              )
            )

          )
      )
    )
  );


  updateInvestmentValue$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        InvestmentActions.updateInvestmentValue
      ),

      mergeMap(action =>
        this.investmentService
          .updateCurrentValue(
            action.id,
            action.currentValue
          )
          .pipe(

            map(response =>
              InvestmentActions.updateInvestmentValueSuccess({
                message:
                  response?.message ??
                  'Investment value updated successfully.'
              })
            ),

            catchError(error =>
              of(
                InvestmentActions.updateInvestmentValueFailure({
                  error: this.getErrorMessage(error)
                })
              )
            )

          )
      )
    )
  );


  sellInvestment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        InvestmentActions.sellInvestment
      ),

      mergeMap(action =>
        this.investmentService
          .sellInvestment(
            action.id,
            {
              accountId: action.accountId,
              sellAmount: action.sellAmount,
              sellDate: action.sellDate,
              description: action.description
            }
          )
          .pipe(

            map(response =>
              InvestmentActions.sellInvestmentSuccess({
                message:
                  response?.message ??
                  'Investment sold successfully.'
              })
            ),

            catchError(error =>
              of(
                InvestmentActions.sellInvestmentFailure({
                  error: this.getErrorMessage(error)
                })
              )
            )

          )
      )
    )
  );


  deleteInvestment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        InvestmentActions.deleteInvestment
      ),

      mergeMap(action =>
        this.investmentService
          .deleteInvestment(action.id)
          .pipe(

            map(response =>
              InvestmentActions.deleteInvestmentSuccess({
                message:
                  response?.message ??
                  'Investment deleted successfully.'
              })
            ),

            catchError(error =>
              of(
                InvestmentActions.deleteInvestmentFailure({
                  error: this.getErrorMessage(error)
                })
              )
            )

          )
      )
    )
  );


 reloadAfterCreate$ = createEffect(() =>
  this.actions$.pipe(
    ofType(
      InvestmentActions.createInvestmentSuccess
    ),

    mergeMap(() => [
      InvestmentActions.loadInvestments(),
      InvestmentActions.loadInvestmentSummary(),
      NetWorthActions.loadNetWorth()
    ])
  )
);


 reloadAfterUpdate$ = createEffect(() =>
  this.actions$.pipe(
    ofType(
      InvestmentActions.updateInvestmentValueSuccess
    ),

    mergeMap(() => [
      InvestmentActions.loadInvestments(),
      InvestmentActions.loadInvestmentSummary(),
      NetWorthActions.loadNetWorth()
    ])
  )
);


  reloadAfterSell$ = createEffect(() =>
  this.actions$.pipe(
    ofType(
      InvestmentActions.sellInvestmentSuccess
    ),

    mergeMap(() => [
      InvestmentActions.loadInvestments(),
      InvestmentActions.loadInvestmentSummary(),
      NetWorthActions.loadNetWorth()
    ])
  )
);


  reloadAfterDelete$ = createEffect(() =>
  this.actions$.pipe(
    ofType(
      InvestmentActions.deleteInvestmentSuccess
    ),

    mergeMap(() => [
      InvestmentActions.loadInvestments(),
      InvestmentActions.loadInvestmentSummary(),
      NetWorthActions.loadNetWorth()
    ])
  )
);

  private getErrorMessage(error: any): string {

    return (
      error?.error?.message ||
      error?.error?.title ||
      'Something went wrong.'
    );
  }
}