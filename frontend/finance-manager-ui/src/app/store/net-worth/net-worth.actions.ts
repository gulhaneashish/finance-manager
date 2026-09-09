import { createAction, props } from '@ngrx/store';

import { NetWorth } from '../../core/models/net-worth.model';

export const loadNetWorth = createAction(
  '[Net Worth] Load Net Worth'
);

export const loadNetWorthSuccess = createAction(
  '[Net Worth API] Load Net Worth Success',
  props<{
    netWorth: NetWorth;
  }>()
);

export const loadNetWorthFailure = createAction(
  '[Net Worth API] Load Net Worth Failure',
  props<{
    error: string;
  }>()
);