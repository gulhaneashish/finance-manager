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
  switchMap,
  tap
} from 'rxjs';

import * as AccountsActions
  from './accounts.actions';

import {
  AccountService
} from '../../core/services/account';

@Injectable()
export class AccountsEffects {

  private actions$ = inject(Actions);

  private accountService =
    inject(AccountService);

loadAccounts$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      AccountsActions.loadAccounts
    ),

    tap(() =>
      console.log('🔥 Accounts effect fired')
    ),

    switchMap(() =>
      this.accountService.getAll().pipe(

        map(accounts =>
          AccountsActions.loadAccountsSuccess({
            accounts
          })
        ),

        catchError(error =>
          of(
            AccountsActions.loadAccountsFailure({
              error:
                error.error?.message ??
                'Failed to load accounts.'
            })
          )
        )

      )
    )
  )
);

loadActiveAccounts$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      AccountsActions.loadActiveAccounts
    ),

    tap(() =>
      console.log('🔥 Accounts effect fired')
    ),

    switchMap(() =>
      this.accountService.getActive().pipe(

        map(accounts =>
          AccountsActions.loadActiveAccountsSuccess({
            accounts
          })
        ),

        catchError(error =>
          of(
            AccountsActions.loadActiveAccountsFailure({
              error:
                error.error?.message ??
                'Failed to load active accounts.'
            })
          )
        )

      )
    )
  )
);
createAccount$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      AccountsActions.createAccount
    ),

    switchMap(({ account }) =>
      this.accountService
        .create(account)
        .pipe(

          map(createdAccount =>
            AccountsActions.createAccountSuccess({
              account: createdAccount
            })
          ),

          catchError(error =>
            of(
              AccountsActions.createAccountFailure({
                error:
                  error.error?.message ??
                  'Failed to create account.'
              })
            )
          )

        )
    )

  )
);

updateAccount$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      AccountsActions.updateAccount
    ),

    switchMap(({ id, account }) =>
      this.accountService
        .update(id, account)
        .pipe(

          map(updatedAccount =>
            AccountsActions.updateAccountSuccess({
              account: updatedAccount
            })
          ),

          catchError(error =>
            of(
              AccountsActions.updateAccountFailure({
                error:
                  error.error?.message ??
                  'Failed to update account.'
              })
            )
          )

        )
    )

  )
);
deleteAccount$ = createEffect(() =>
  this.actions$.pipe(

    ofType(
      AccountsActions.deleteAccount
    ),

    switchMap(({ id }) =>
      this.accountService.delete(id).pipe(

        map(() =>
          AccountsActions.deleteAccountSuccess({
            id
          })
        ),

        catchError(error =>
          of(
            AccountsActions.deleteAccountFailure({
              error:
                error.error?.message ??
                'Failed to delete account.'
            })
          )
        )

      )
    )

  )
);
}