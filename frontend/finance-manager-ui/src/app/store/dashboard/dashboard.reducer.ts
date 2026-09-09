import {
  createReducer,
  on
} from '@ngrx/store';

import {
  loadDashboard,
  loadDashboardSuccess,
  loadDashboardFailure,
  loadCategorySpending,
  loadCategorySpendingSuccess,
  loadCategorySpendingFailure,
  loadMonthlyCashFlowFailure,
  loadMonthlyCashFlowSuccess,
  loadMonthlyCashFlow,
  loadLoanDebtFailure,
  loadLoanDebtSuccess,
  loadLoanDebt,
  loadSavingsInvestmentsFailure,
  loadSavingsInvestmentsSuccess,
  loadSavingsInvestments
} from './dashboard.actions';

import {
  DashboardSummary,
  CategorySpending,
  MonthlyCashFlow,
  AccountSummary,
  SavingsInvestmentSummary
} from '../../core/models/dashboard.model';
import { LoanDebtSummary } from '../../core/models/loan-debt.model';

import {

  loadAccounts,
  loadAccountsSuccess,
  loadAccountsFailure
} from './dashboard.actions';
export interface DashboardState {

  data: DashboardSummary | null;

  categorySpending: CategorySpending[];

  loading: boolean;
 accounts: AccountSummary[];
  categorySpendingLoading: boolean;
savingsInvestments:
  SavingsInvestmentSummary | null;
  error: string | null;
loanDebt: LoanDebtSummary | null;
  categorySpendingError: string | null;
  monthlyCashFlow: MonthlyCashFlow[];
monthlyCashFlowLoading: boolean;
monthlyCashFlowError: string | null;
}


export const initialDashboardState: DashboardState = {

  data: null,

  categorySpending: [],

  loading: false,
accounts: [],
  categorySpendingLoading: false,
  loanDebt: null,
  error: null,
 savingsInvestments: null,
  categorySpendingError: null,
  monthlyCashFlow: [],
  monthlyCashFlowLoading: false,
  monthlyCashFlowError: null
};


export const dashboardReducer = createReducer(

  initialDashboardState,


  // ===============================
  // Summary
  // ===============================

  on(loadDashboard, state => ({

    ...state,

    loading: true,

    error: null

  })),


  on(
    loadDashboardSuccess,
    (state, { dashboard }) => ({

      ...state,

      data: dashboard,

      loading: false,

      error: null

    })
  ),


  on(
    loadDashboardFailure,
    (state, { error }) => ({

      ...state,

      loading: false,

      error

    })
  ),


  // ===============================
  // Category Spending
  // ===============================

  on(
    loadCategorySpending,
    state => ({

      ...state,

      categorySpendingLoading: true,

      categorySpendingError: null

    })
  ),


  on(
    loadCategorySpendingSuccess,
    (state, { categorySpending }) => ({

      ...state,

      categorySpending,

      categorySpendingLoading: false,

      categorySpendingError: null

    })
  ),


  on(
    loadCategorySpendingFailure,
    (state, { error }) => ({

      ...state,

      categorySpendingLoading: false,

      categorySpendingError: error

    })
  ),
  on(
  loadMonthlyCashFlow,
  state => ({
    ...state,
    monthlyCashFlowLoading: true,
    monthlyCashFlowError: null
  })
),

on(
  loadMonthlyCashFlowSuccess,
  (state, { monthlyCashFlow }) => ({
    ...state,
    monthlyCashFlow,
    monthlyCashFlowLoading: false,
    monthlyCashFlowError: null
  })
),

on(
  loadMonthlyCashFlowFailure,
  (state, { error }) => ({
    ...state,
    monthlyCashFlowLoading: false,
    monthlyCashFlowError: error
  })
),
on(
  loadLoanDebt,
  state => ({
    ...state,
    loading: true,
    error: null
  })
),

on(
  loadLoanDebtSuccess,
  (state, { loanDebt }) => ({
    ...state,
    loanDebt,
    loading: false,
    error: null
  })
),

on(
  loadLoanDebtFailure,
  (state, { error }) => ({
    ...state,
    loading: false,
    error
  })
),
on(loadAccounts, state => ({
  ...state,
  loading: true,
  error: null
})),

on(loadAccountsSuccess, (state, { accounts }) => ({
  ...state,
  accounts,
  loading: false,
  error: null
})),

on(loadAccountsFailure, (state, { error }) => ({
  ...state,
  loading: false,
  error
})),
on(
  loadSavingsInvestments,
  state => ({
    ...state,
    loading: true,
    error: null
  })
),

on(
  loadSavingsInvestmentsSuccess,
  (state, { savingsInvestments }) => ({
    ...state,
    savingsInvestments,
    loading: false,
    error: null
  })
),

on(
  loadSavingsInvestmentsFailure,
  (state, { error }) => ({
    ...state,
    loading: false,
    error
  })
)
);