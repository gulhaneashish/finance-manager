import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient , withFetch, withInterceptors} from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth-interceptor';
import { routes } from './app.routes';
import { provideState, provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import {
  accountsReducer
} from './store/accounts/accounts.reducer';

import {
  AccountsEffects
} from './store/accounts/accounts.effects';
import {
  dashboardReducer
} from './store/dashboard/dashboard.reducer';
import {
  APP_INITIALIZER
} from '@angular/core';


import {
  AuthInitializerService
} from './core/services/auth-initializer.service';
import {
  DashboardEffects
} from './store/dashboard/dashboard.effects';
import { authReducer } from './store/auth/auth.reducer';
import { transactionReducer } from './store/transactions/transaction.reducer';
import { TransactionEffects } from './store/transactions/transaction.effects';
import { categoryReducer } from './store/categories/category.reducer';
import { CategoryEffects } from './store/categories/category.effects';
import { budgetReducer } from './store/budgets/budget.reducer';
import { BudgetEffects } from './store/budgets/budget.effects';
import { loanReducer } from './store/loans/loan.reducer';
import { LoanEffects } from './store/loans/loan.effects';
import { ReportEffects } from './store/report/report.effects';
import { reportReducer } from './store/report/report.reducer';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { creditCardReducer } from './store/credit-card/credit-card.reducer';
import { CreditCardEffects } from './store/credit-card/credit-card.effects';
import { MonthlyReportEffects } from './store/monthly-report/monthly-report.effects';
import { monthlyReportReducer } from './store/monthly-report/monthly-report.reducer';
import { netWorthReducer } from './store/net-worth/net-worth.reducer';
import { NetWorthEffects } from './store/net-worth/net-worth.effects';
import { InvestmentEffects } from './store/investment/investment.effects';
import { investmentReducer } from './store/investment/investment.reducer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([
        authInterceptor
      ])
    ),
  provideStore({
  auth: authReducer,
  dashboard: dashboardReducer,
  accounts: accountsReducer,
  transactions: transactionReducer,
  categories: categoryReducer,
  budget: budgetReducer,
  loans: loanReducer,
   report: reportReducer,
  'credit-card': creditCardReducer,
 monthlyReport: monthlyReportReducer,
 netWorth: netWorthReducer,
 investment: investmentReducer
}),
 
  provideEffects([
    DashboardEffects,
    AccountsEffects,
    TransactionEffects,
    CategoryEffects,
    BudgetEffects,
    LoanEffects,
    ReportEffects,
    CreditCardEffects,
    MonthlyReportEffects,
    NetWorthEffects,
    InvestmentEffects
  ]),

  {
  provide: APP_INITIALIZER,
  useFactory: (
    initializer: AuthInitializerService
  ) => () => initializer.initialize(),
  deps: [AuthInitializerService],
  multi: true
},
provideCharts(
      withDefaultRegisterables()
    ),
    provideState(
  'credit-card',
  creditCardReducer
)
  ]
};