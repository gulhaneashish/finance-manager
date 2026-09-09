import { createAction, props } from '@ngrx/store';

import {
  DashboardSummary,
  CategorySpending,
  MonthlyCashFlow,
  AccountSummary,
  SavingsInvestmentSummary
} from '../../core/models/dashboard.model';
import { LoanDebtSummary } from '../../core/models/loan-debt.model';


// ===============================
// Dashboard Summary
// ===============================

export const loadDashboard = createAction(
  '[Dashboard] Load Dashboard',
  props<{
    year: number;
    month: number;
  }>()
);

export const loadDashboardSuccess = createAction(
  '[Dashboard API] Load Dashboard Success',
  props<{
    dashboard: DashboardSummary;
  }>()
);

export const loadDashboardFailure = createAction(
  '[Dashboard API] Load Dashboard Failure',
  props<{
    error: string;
  }>()
);


// ===============================
// Category Spending
// ===============================

export const loadCategorySpending = createAction(
  '[Dashboard] Load Category Spending',
  props<{
    year: number;
    month: number;
  }>()
);

export const loadCategorySpendingSuccess = createAction(
  '[Dashboard API] Load Category Spending Success',
  props<{
    categorySpending: CategorySpending[];
  }>()
);

export const loadCategorySpendingFailure = createAction(
  '[Dashboard API] Load Category Spending Failure',
  props<{
    error: string;
  }>()
);

export const loadMonthlyCashFlow = createAction(
  '[Dashboard] Load Monthly Cash Flow',
  props<{
    year: number;
  }>()
);

export const loadMonthlyCashFlowSuccess = createAction(
  '[Dashboard API] Load Monthly Cash Flow Success',
  props<{
    monthlyCashFlow: MonthlyCashFlow[];
  }>()
);

export const loadMonthlyCashFlowFailure = createAction(
  '[Dashboard API] Load Monthly Cash Flow Failure',
  props<{
    error: string;
  }>()
);

export const loadLoanDebt = createAction(
  '[Dashboard] Load Loan Debt'
);

export const loadLoanDebtSuccess = createAction(
  '[Dashboard API] Load Loan Debt Success',
  props<{
    loanDebt: LoanDebtSummary;
  }>()
);

export const loadLoanDebtFailure = createAction(
  '[Dashboard API] Load Loan Debt Failure',
  props<{
    error: string;
  }>()
);

export const loadAccounts = createAction(
  '[Dashboard] Load Accounts'
);

export const loadAccountsSuccess = createAction(
  '[Dashboard API] Load Accounts Success',
  props<{
    accounts: AccountSummary[];
  }>()
);

export const loadAccountsFailure = createAction(
  '[Dashboard API] Load Accounts Failure',
  props<{
    error: string;
  }>()
);

export const loadSavingsInvestments =
  createAction(
    '[Dashboard] Load Savings Investments',
    props<{
      year: number;
      month: number;
    }>()
  );

export const loadSavingsInvestmentsSuccess =
  createAction(
    '[Dashboard API] Load Savings Investments Success',
    props<{
      savingsInvestments:
        SavingsInvestmentSummary;
    }>()
  );

export const loadSavingsInvestmentsFailure =
  createAction(
    '[Dashboard API] Load Savings Investments Failure',
    props<{
      error: string;
    }>()
  );