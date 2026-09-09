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

import * as LoanActions from './loan.actions';
import { LoanService } from '../../core/services/loan.service';

@Injectable()
export class LoanEffects {

  private actions$ = inject(Actions);
  private loanService = inject(LoanService);

  loadLoans$ = createEffect(() =>
    this.actions$.pipe(

      ofType(LoanActions.loadLoans),

      switchMap(() =>
        this.loanService.getAll().pipe(

          map(loans =>
            LoanActions.loadLoansSuccess({
              loans
            })
          ),

          catchError(error =>
            of(
              LoanActions.loadLoansFailure({
                error:
                  error.error?.message ||
                  'Failed to load loans.'
              })
            )
          )

        )
      )

    )
  );


  createLoan$ = createEffect(() =>
    this.actions$.pipe(

      ofType(LoanActions.createLoan),

      switchMap(({ loan }) =>
        this.loanService.create(loan).pipe(

          map(createdLoan =>
            LoanActions.createLoanSuccess({
              loan: createdLoan
            })
          ),

          catchError(error =>
            of(
              LoanActions.createLoanFailure({
                error:
                  error.error?.message ||
                  'Failed to create loan.'
              })
            )
          )

        )
      )

    )
  );


  addLoanPayment$ = createEffect(() =>
    this.actions$.pipe(

      ofType(LoanActions.addLoanPayment),

      switchMap(({ loanId, payment }) =>
        this.loanService
          .addPayment(loanId, payment)
          .pipe(

            map(updatedLoan =>
              LoanActions.addLoanPaymentSuccess({
                loan: updatedLoan
              })
            ),

            catchError(error =>
              of(
                LoanActions.addLoanPaymentFailure({
                  error:
                    error.error?.message ||
                    'Failed to add loan payment.'
                })
              )
            )

          )
      )

    )
  );
updateLoan$ = createEffect(() =>
  this.actions$.pipe(
    ofType(LoanActions.updateLoan),

    switchMap(({ loanId, loan }) =>
      this.loanService
        .update(loanId, loan)
        .pipe(

          map(updatedLoan =>
            LoanActions.updateLoanSuccess({
              loan: updatedLoan
            })
          ),

          catchError(error =>
            of(
              LoanActions.updateLoanFailure({
                error:
                  error.error?.message ||
                  'Failed to update loan.'
              })
            )
          )

        )
    )
  )
);
}