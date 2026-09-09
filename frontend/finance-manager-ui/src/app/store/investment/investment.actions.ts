import { createAction, props } from '@ngrx/store';

import {
  Investment,
  InvestmentSummary,
  InvestmentType
} from '../../core/models/investment.model';

export const loadInvestments = createAction(
  '[Investment] Load Investments'
);

export const loadInvestmentsSuccess = createAction(
  '[Investment] Load Investments Success',
  props<{ investments: Investment[] }>()
);

export const loadInvestmentsFailure = createAction(
  '[Investment] Load Investments Failure',
  props<{ error: string }>()
);

export const loadInvestmentSummary = createAction(
  '[Investment] Load Investment Summary'
);

export const loadInvestmentSummarySuccess = createAction(
  '[Investment] Load Investment Summary Success',
  props<{ summary: InvestmentSummary }>()
);

export const loadInvestmentSummaryFailure = createAction(
  '[Investment] Load Investment Summary Failure',
  props<{ error: string }>()
);

export const createInvestment = createAction(
  '[Investment] Create Investment',
  props<{
    accountId: number;
    name: string;
    investmentType: InvestmentType;
    amount: number;
    investmentDate: string;
    description?: string;
  }>()
);

export const createInvestmentSuccess = createAction(
  '[Investment] Create Investment Success',
  props<{ message: string }>()
);

export const createInvestmentFailure = createAction(
  '[Investment] Create Investment Failure',
  props<{ error: string }>()
);

export const updateInvestmentValue = createAction(
  '[Investment] Update Investment Value',
  props<{
    id: number;
    currentValue: number;
  }>()
);

export const updateInvestmentValueSuccess = createAction(
  '[Investment] Update Investment Value Success',
  props<{ message: string }>()
);

export const updateInvestmentValueFailure = createAction(
  '[Investment] Update Investment Value Failure',
  props<{ error: string }>()
);

export const sellInvestment = createAction(
  '[Investment] Sell Investment',
  props<{
    id: number;
    accountId: number;
    sellAmount: number;
    sellDate: string;
    description?: string;
  }>()
);

export const sellInvestmentSuccess = createAction(
  '[Investment] Sell Investment Success',
  props<{ message: string }>()
);

export const sellInvestmentFailure = createAction(
  '[Investment] Sell Investment Failure',
  props<{ error: string }>()
);

export const deleteInvestment = createAction(
  '[Investment] Delete Investment',
  props<{ id: number }>()
);

export const deleteInvestmentSuccess = createAction(
  '[Investment] Delete Investment Success',
  props<{ message: string }>()
);

export const deleteInvestmentFailure = createAction(
  '[Investment] Delete Investment Failure',
  props<{ error: string }>()
);