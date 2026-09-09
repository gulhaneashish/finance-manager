import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [

  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(m => m.Login)
  },

  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register')
        .then(m => m.Register)
  },

  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./shared/components/layout/layout')
        .then(m => m.Layout),

    children: [

      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },

      {
        path: 'dashboard',
        loadComponent: () =>
          import(
            './features/dashboard/dashboard/dashboard'
          ).then(m => m.Dashboard)
      },

      {
        path: 'transactions',
        loadComponent: () =>
          import(
            './features/transactions/transaction-list/transaction-list'
          ).then(m => m.TransactionList)
      },

      {
        path: 'accounts',
        loadComponent: () =>
          import(
            './features/accounts/account-list/account-list'
          ).then(m => m.AccountList)
      },

      {
        path: 'loans',
        loadComponent: () =>
          import(
            './features/loans/loan-list/loan-list'
          ).then(m => m.LoanList)
      },

      {
        path: 'budgets',
        loadComponent: () =>
          import(
            './features/budgets/budget-page/budget-page'
          ).then(m => m.BudgetPage)
      },

      {
        path: 'categories',
        loadComponent: () =>
          import(
            './features/categories/category-list/category-list'
          ).then(m => m.CategoryList)
      },

      {
        path: 'reports',
        loadComponent: () =>
          import('./features/reports/reports/reports')
            .then(m => m.Reports),

        children: [

          {
            path: '',
            redirectTo: 'expense',
            pathMatch: 'full'
          },

          {
            path: 'expense',
            loadComponent: () =>
              import(
                './features/reports/expense-report/expense-report/expense-report'
              ).then(m => m.ExpenseReport)
          },

          {
            path: 'monthly',
            loadComponent: () =>
              import(
                './features/monthly-report/monthly-report-page/monthly-report-page'
              ).then(m => m.MonthlyReportPage)
          },

          {
            path: 'net-worth',
            loadComponent: () =>
              import(
                './features/reports/net-worth/net-worth-page/net-worth-page'
              ).then(m => m.NetWorthPage)
          }

        ]
      },

      {
        path: 'investments',
        loadComponent: () =>
          import(
            './features/investments/investment-list/investment-list'
          ).then(m => m.InvestmentList)
      },

      {
        path: 'investments/add',
        loadComponent: () =>
          import(
            './features/investments/investment-form/investment-form'
          ).then(m => m.InvestmentForm)
      }

    ]
  },

  {
    path: '**',
    redirectTo: 'dashboard'
  }

];