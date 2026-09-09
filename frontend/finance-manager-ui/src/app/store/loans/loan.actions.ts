import { createAction, props } from '@ngrx/store';
import {
  Loan,
  LoanCreate,
  LoanPaymentCreate,
  LoanUpdate
} from '../../core/models/loan.model';

export const loadLoans = createAction(
  '[Loan] Load Loans'
);

export const loadLoansSuccess = createAction(
  '[Loan] Load Loans Success',
  props<{
    loans: Loan[];
  }>()
);

export const loadLoansFailure = createAction(
  '[Loan] Load Loans Failure',
  props<{
    error: string;
  }>()
);

export const createLoan = createAction(
  '[Loan] Create Loan',
  props<{
    loan: LoanCreate;
  }>()
);

export const createLoanSuccess = createAction(
  '[Loan] Create Loan Success',
  props<{
    loan: Loan;
  }>()
);

export const createLoanFailure = createAction(
  '[Loan] Create Loan Failure',
  props<{
    error: string;
  }>()
);

export const addLoanPayment = createAction(
  '[Loan] Add Loan Payment',
  props<{
    loanId: number;
    payment: LoanPaymentCreate;
  }>()
);

export const addLoanPaymentSuccess = createAction(
  '[Loan] Add Loan Payment Success',
  props<{
    loan: Loan;
  }>()
);

export const addLoanPaymentFailure = createAction(
  '[Loan] Add Loan Payment Failure',
  props<{
    error: string;
  }>()
);

export const updateLoan = createAction(
  '[Loan] Update Loan',
  props<{
    loanId: number;
    loan: LoanUpdate;
  }>()
);

export const updateLoanSuccess = createAction(
  '[Loan] Update Loan Success',
  props<{
    loan: Loan;
  }>()
);

export const updateLoanFailure = createAction(
  '[Loan] Update Loan Failure',
  props<{
    error: string;
  }>()
);