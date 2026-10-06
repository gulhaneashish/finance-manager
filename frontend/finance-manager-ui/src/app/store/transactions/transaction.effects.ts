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

import { loadAccounts } from '../accounts/accounts.actions';
import { TransactionService } from '../../core/services/transaction.service';
import * as TransactionActions from './transaction.actions';
import * as NetWorthActions from '../net-worth/net-worth.actions';

@Injectable()
export class TransactionEffects {

  private actions$ = inject(Actions);

  private transactionService =
    inject(TransactionService);

  loadTransactions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        TransactionActions.loadTransactions
      ),
      switchMap(() =>
        this.transactionService
          .getAll()
          .pipe(
            map(transactions =>
              TransactionActions
                .loadTransactionsSuccess({
                  transactions
                })
            ),
            catchError(error =>
              of(
                TransactionActions
                  .loadTransactionsFailure({
                    error:
                      error.error?.message ??
                      'Failed to load transactions.'
                  })
              )
            )
          )
      )
    )
  );

  createTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        TransactionActions.createTransaction
      ),
      switchMap(({ transaction }) =>
        this.transactionService
          .create(transaction)
          .pipe(
            map(createdTransaction =>
              TransactionActions
                .createTransactionSuccess({
                  transaction:
                    createdTransaction
                })
            ),
            catchError(error => {
              const errorMessage =
                error.error?.detail ||
                error.error?.message ||
                error.error?.title ||
                'Failed to create transaction.';
              return of(
                TransactionActions
                  .createTransactionFailure({
                    error: errorMessage
                  })
              );
            })
          )
      )
    )
  );

  deleteTransaction$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        TransactionActions.deleteTransaction
      ),
      switchMap(({ id }) =>
        this.transactionService
          .delete(id)
          .pipe(
            map(() =>
              TransactionActions
                .deleteTransactionSuccess({
                  id
                })
            ),
            catchError(error =>
              of(
                TransactionActions
                  .deleteTransactionFailure({
                    error:
                      error.error?.message ??
                      'Failed to delete transaction.'
                  })
              )
            )
          )
      )
    )
  );

  createTransfer$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        TransactionActions.createTransfer
      ),
      switchMap(({ transfer }) =>
        this.transactionService
          .transfer(transfer)
          .pipe(
            map(() =>
              TransactionActions
                .createTransferSuccess()
            ),
            catchError(error =>
              of(
                TransactionActions
                  .createTransferFailure({
                    error:
                      error.error?.message ??
                      'Transfer failed.'
                  })
              )
            )
          )
      )
    )
  );

  refreshAfterTransfer$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        TransactionActions.createTransferSuccess
      ),
      switchMap(() => [
        TransactionActions.loadTransactions(),
        loadAccounts(),
        NetWorthActions.loadNetWorth()
      ])
    )
  );

  filterTransactions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        TransactionActions.filterTransactions
      ),
      switchMap(({ filter }) =>
        this.transactionService
          .getFiltered(filter)
          .pipe(
            map(transactions =>
              TransactionActions
                .filterTransactionsSuccess({
                  transactions
                })
            ),
            catchError(error =>
              of(
                TransactionActions
                  .filterTransactionsFailure({
                    error:
                      error.error?.message ??
                      'Failed to filter transactions.'
                  })
              )
            )
          )
      )
    )
  );
}