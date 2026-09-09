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

import * as CreditCardActions
  from './credit-card.actions';

import { CreditCardService }
  from '../../core/services/credit-card.service';

import { loadAccounts }
  from '../accounts/accounts.actions';

@Injectable()
export class CreditCardEffects {

  private actions$ = inject(Actions);

  private creditCardService =
    inject(CreditCardService);


  // =========================
  // MAKE PURCHASE
  // =========================

  makePurchase$ = createEffect(() =>
    this.actions$.pipe(

      ofType(
        CreditCardActions.makePurchase
      ),

      mergeMap(({ purchase }) =>

        this.creditCardService
          .purchase(purchase)

          .pipe(

            map(response =>
              CreditCardActions.makePurchaseSuccess({
                response
              })
            ),

            catchError(error =>
              of(
                CreditCardActions.makePurchaseFailure({
                  error:
                    error?.error?.message ??
                    'Failed to make credit card purchase.'
                })
              )
            )

          )

      )

    )
  );


  // =========================
  // REFRESH ACCOUNTS
  // AFTER PURCHASE
  // =========================

  refreshAccountsAfterPurchase$ =
    createEffect(() =>
      this.actions$.pipe(

        ofType(
          CreditCardActions.makePurchaseSuccess
        ),

        map(() =>
          loadAccounts()
        )

      )
    );


  // =========================
  // MAKE PAYMENT
  // =========================

  makePayment$ = createEffect(() =>
    this.actions$.pipe(

      ofType(
        CreditCardActions.makePayment
      ),

      mergeMap(({ payment }) =>

        this.creditCardService
          .payment(payment)

          .pipe(

            map(response =>
              CreditCardActions.makePaymentSuccess({
                response
              })
            ),

            catchError(error =>
              of(
                CreditCardActions.makePaymentFailure({
                  error:
                    error?.error?.message ??
                    'Failed to make credit card payment.'
                })
              )
            )

          )

      )

    )
  );


  // =========================
  // REFRESH ACCOUNTS
  // AFTER PAYMENT
  // =========================

  refreshAccountsAfterPayment$ =
    createEffect(() =>
      this.actions$.pipe(

        ofType(
          CreditCardActions.makePaymentSuccess
        ),

        map(() =>
          loadAccounts()
        )

      )
    );

}