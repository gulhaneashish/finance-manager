import { createAction, props } from '@ngrx/store';

import {
  CreditCardPurchaseCreate,
  CreditCardPurchaseResponse,
  CreditCardPaymentCreate,
  CreditCardPaymentResponse
} from '../../core/models/credit-card.model';

export const makePurchase = createAction(
  '[Credit Card] Make Purchase',
  props<{
    purchase: CreditCardPurchaseCreate;
  }>()
);

export const makePurchaseSuccess = createAction(
  '[Credit Card] Make Purchase Success',
  props<{
    response: CreditCardPurchaseResponse;
  }>()
);

export const makePurchaseFailure = createAction(
  '[Credit Card] Make Purchase Failure',
  props<{
    error: string;
  }>()
);

export const makePayment = createAction(
  '[Credit Card] Make Payment',
  props<{
    payment: CreditCardPaymentCreate;
  }>()
);

export const makePaymentSuccess = createAction(
  '[Credit Card] Make Payment Success',
  props<{
    response: CreditCardPaymentResponse;
  }>()
);

export const makePaymentFailure = createAction(
  '[Credit Card] Make Payment Failure',
  props<{
    error: string;
  }>()
);