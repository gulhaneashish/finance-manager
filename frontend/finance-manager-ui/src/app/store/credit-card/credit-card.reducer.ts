import { createReducer, on } from '@ngrx/store';

import {
  makePurchase,
  makePurchaseSuccess,
  makePurchaseFailure,
  makePayment,
  makePaymentSuccess,
  makePaymentFailure
} from './credit-card.actions';

export interface CreditCardState {
  loading: boolean;
  error: string | null;
  successMessage: string | null;
}

export const initialState: CreditCardState = {
  loading: false,
  error: null,
  successMessage: null
};

export const creditCardReducer = createReducer(

  initialState,

  on(
    makePurchase,
    state => ({
      ...state,
      loading: true,
      error: null,
      successMessage: null
    })
  ),

  on(
    makePurchaseSuccess,
    state => ({
      ...state,
      loading: false,
      error: null,
      successMessage:
        'Credit card purchase completed successfully.'
    })
  ),

  on(
    makePurchaseFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      successMessage: null
    })
  ),

  on(
    makePayment,
    state => ({
      ...state,
      loading: true,
      error: null,
      successMessage: null
    })
  ),

  on(
    makePaymentSuccess,
    state => ({
      ...state,
      loading: false,
      error: null,
      successMessage:
        'Credit card payment completed successfully.'
    })
  ),

  on(
    makePaymentFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      successMessage: null
    })
  )

);