import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';

import { Auth } from './core/services/auth';
import { authGuard } from './core/guards/auth-guard';
import { adminGuard } from './core/guards/admin.guard';
import { userGuard } from './core/guards/user.guard';

const rootRedirectGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);
  return auth.isAdmin()
    ? router.createUrlTree(['/admin'])
    : router.createUrlTree(['/dashboard']);
};

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
        pathMatch: 'full',
        canActivate: [rootRedirectGuard],
        children: []
      },

      {
        path: 'dashboard',
        canActivate: [userGuard],
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
        redirectTo: 'investments',
        pathMatch: 'full'
      },

      {
        path: 'admin',
        canActivate: [adminGuard],
        loadComponent: () =>
          import(
            './features/admin/admin-portal/admin-portal'
          ).then(m => m.AdminPortal)
      }

    ]
  },

  {
    path: '**',
    redirectTo: 'dashboard'
  }

];