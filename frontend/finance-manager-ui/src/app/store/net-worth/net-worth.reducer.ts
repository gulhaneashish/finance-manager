import { createReducer, on } from '@ngrx/store';

import {
  loadNetWorth,
  loadNetWorthSuccess,
  loadNetWorthFailure
} from './net-worth.actions';

import { NetWorth } from '../../core/models/net-worth.model';

export interface NetWorthState {
  netWorth: NetWorth | null;
  loading: boolean;
  error: string | null;
}

export const initialState: NetWorthState = {
  netWorth: null,
  loading: false,
  error: null
};

export const netWorthReducer = createReducer(

  initialState,

  on(
    loadNetWorth,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    loadNetWorthSuccess,
    (state, { netWorth }) => ({
      ...state,
      netWorth,
      loading: false,
      error: null
    })
  ),

  on(
    loadNetWorthFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  )

);