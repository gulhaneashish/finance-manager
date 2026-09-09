import { createReducer, on } from '@ngrx/store';

import {
  Investment,
  InvestmentSummary
} from '../../core/models/investment.model';

import * as InvestmentActions from './investment.actions';

export interface InvestmentState {
  investments: Investment[];
  summary: InvestmentSummary | null;
  loading: boolean;
  error: string | null;
  operationSuccess: string | null;
}

export const initialInvestmentState: InvestmentState = {
  investments: [],
  summary: null,
  loading: false,
  error: null,
  operationSuccess: null
};

export const investmentReducer = createReducer(
  initialInvestmentState,

  // =========================
  // LOAD INVESTMENTS
  // =========================

  on(
    InvestmentActions.loadInvestments,
    (state) => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    InvestmentActions.loadInvestmentsSuccess,
    (state, { investments }) => ({
      ...state,
      investments,
      loading: false,
      error: null
    })
  ),

  on(
    InvestmentActions.loadInvestmentsFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

  // =========================
  // LOAD SUMMARY
  // =========================

  on(
    InvestmentActions.loadInvestmentSummary,
    (state) => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    InvestmentActions.loadInvestmentSummarySuccess,
    (state, { summary }) => ({
      ...state,
      summary,
      loading: false,
      error: null
    })
  ),

  on(
    InvestmentActions.loadInvestmentSummaryFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

  // =========================
  // CREATE
  // =========================

  on(
    InvestmentActions.createInvestment,
    (state) => ({
      ...state,
      loading: true,
      error: null,
      operationSuccess: null
    })
  ),

  on(
    InvestmentActions.createInvestmentSuccess,
    (state, { message }) => ({
      ...state,
      loading: false,
      operationSuccess: message,
      error: null
    })
  ),

  on(
    InvestmentActions.createInvestmentFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      operationSuccess: null
    })
  ),

  // =========================
  // UPDATE VALUE
  // =========================

  on(
    InvestmentActions.updateInvestmentValue,
    (state) => ({
      ...state,
      loading: true,
      error: null,
      operationSuccess: null
    })
  ),

  on(
    InvestmentActions.updateInvestmentValueSuccess,
    (state, { message }) => ({
      ...state,
      loading: false,
      operationSuccess: message,
      error: null
    })
  ),

  on(
    InvestmentActions.updateInvestmentValueFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      operationSuccess: null
    })
  ),

  // =========================
  // SELL
  // =========================

  on(
    InvestmentActions.sellInvestment,
    (state) => ({
      ...state,
      loading: true,
      error: null,
      operationSuccess: null
    })
  ),

  on(
    InvestmentActions.sellInvestmentSuccess,
    (state, { message }) => ({
      ...state,
      loading: false,
      operationSuccess: message,
      error: null
    })
  ),

  on(
    InvestmentActions.sellInvestmentFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      operationSuccess: null
    })
  ),

  // =========================
  // DELETE
  // =========================

  on(
    InvestmentActions.deleteInvestment,
    (state) => ({
      ...state,
      loading: true,
      error: null,
      operationSuccess: null
    })
  ),

  on(
    InvestmentActions.deleteInvestmentSuccess,
    (state, { message }) => ({
      ...state,
      loading: false,
      operationSuccess: message,
      error: null
    })
  ),

  on(
    InvestmentActions.deleteInvestmentFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      operationSuccess: null
    })
  )
);