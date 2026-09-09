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

import * as NetWorthActions
  from './net-worth.actions';

import { NetWorthService }
  from '../../core/services/net-worth.service';

@Injectable()
export class NetWorthEffects {

  private actions$ = inject(Actions);

  private netWorthService =
    inject(NetWorthService);

  loadNetWorth$ = createEffect(() =>
    this.actions$.pipe(

      ofType(
        NetWorthActions.loadNetWorth
      ),

      mergeMap(() =>
        this.netWorthService
          .getNetWorth()
          .pipe(

            map(netWorth =>
              NetWorthActions.loadNetWorthSuccess({
                netWorth
              })
            ),

            catchError(error =>
              of(
                NetWorthActions.loadNetWorthFailure({
                  error:
                    error?.error?.message ??
                    'Failed to load net worth.'
                })
              )
            )

          )
      )

    )
  );

}