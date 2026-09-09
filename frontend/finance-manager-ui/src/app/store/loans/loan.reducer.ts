import { createReducer, on } from '@ngrx/store';

import {
  loadLoans,
  loadLoansSuccess,
  loadLoansFailure,
  createLoan,
  createLoanSuccess,
  createLoanFailure,
  addLoanPayment,
  addLoanPaymentSuccess,
  addLoanPaymentFailure,
  updateLoanFailure,
  updateLoanSuccess,
  updateLoan
} from './loan.actions';

import { Loan } from '../../core/models/loan.model';

export interface LoanState {
  loans: Loan[];
  loading: boolean;
  error: string | null;
  successMessage: string | null;
}

export const initialState: LoanState = {
  loans: [],
  loading: false,
  error: null,
  successMessage: null
};

export const loanReducer = createReducer(

  initialState,

  on(
    loadLoans,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    loadLoansSuccess,
    (state, { loans }) => ({
      ...state,
      loans,
      loading: false,
      error: null
    })
  ),

  on(
    loadLoansFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

  on(
    createLoan,
    state => ({
      ...state,
      loading: true,
      error: null,
      successMessage: null
    })
  ),

  on(
    createLoanSuccess,
    (state, { loan }) => ({
      ...state,
      loans: [...state.loans, loan],
      loading: false,
      error: null,
      successMessage: 'Loan created successfully.'
    })
  ),

  on(
    createLoanFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      successMessage: null
    })
  ),

  on(
    addLoanPayment,
    state => ({
      ...state,
      loading: true,
      error: null,
      successMessage: null
    })
  ),

  on(
    addLoanPaymentSuccess,
    (state, { loan }) => ({
      ...state,
      loans: state.loans.map(item =>
        item.id === loan.id
          ? loan
          : item
      ),
      loading: false,
      error: null,
      successMessage: 'Payment added successfully.'
    })
  ),

  on(
    addLoanPaymentFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error,
      successMessage: null
    })
  ),
  on(
  updateLoan,
  state => ({
    ...state,
    loading: true,
    error: null,
    successMessage: null
  })
),

on(
  updateLoanSuccess,
  (state, { loan }) => ({
    ...state,

    loans: state.loans.map(item =>
      item.id === loan.id
        ? loan
        : item
    ),

    loading: false,
    error: null,
    successMessage:
      'Loan updated successfully.'
  })
),

on(
  updateLoanFailure,
  (state, { error }) => ({
    ...state,
    loading: false,
    error,
    successMessage: null
  })
),

);