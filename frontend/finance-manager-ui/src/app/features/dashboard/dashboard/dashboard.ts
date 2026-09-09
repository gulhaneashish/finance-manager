import {
  Component,
  inject,
  OnInit
} from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { LoanType } from '../../../core/models/loan.model'; 
import {
  Store
} from '@ngrx/store';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import {
  loadAccounts,
  loadCategorySpending,
  loadDashboard,
  loadLoanDebt,
  loadMonthlyCashFlow,
  loadSavingsInvestments
} from '../../../store/dashboard/dashboard.actions';
import { loadNetWorth } from '../../../store/net-worth/net-worth.actions';
import {  DecimalPipe } from '@angular/common';
import {
  ChartConfiguration,
  ChartData,
  ChartOptions
} from 'chart.js';
import {
  selectNetWorth,
  selectNetWorthLoading,
  selectNetWorthError
} from '../../../store/net-worth/net-worth.selectors';
import { BaseChartDirective } from 'ng2-charts';
import {
  selectDashboard,
  selectDashboardLoading,
  selectDashboardError,
  selectCategorySpendingError,
  selectCategorySpendingLoading,
  selectCategorySpending,
  selectMonthlyCashFlowError,
  selectMonthlyCashFlowLoading,
  selectMonthlyCashFlow,
  selectLoanDebt,
  selectAccounts,
  selectSavingsInvestments
} from '../../../store/dashboard/dashboard.selectors';
import { CategorySpending, MonthlyCashFlow } from '../../../core/models/dashboard.model';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    AsyncPipe,
    DecimalPipe,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatButtonModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {


  private store = inject(Store);
 LoanType=LoanType;
  dashboard$ =
    this.store.select(selectDashboard);

  loading$ =
    this.store.select(
      selectDashboardLoading
    );
accounts$ =
  this.store.select(selectAccounts);
  error$ =
    this.store.select(
      selectDashboardError
    );
    categorySpending$ =
  this.store.select(selectCategorySpending);
loanDebt$ =
  this.store.select(selectLoanDebt);
savingsInvestments$ =
  this.store.select(
    selectSavingsInvestments
  );

  netWorth$ =
  this.store.select(selectNetWorth);

netWorthLoading$ =
  this.store.select(selectNetWorthLoading);

netWorthError$ =
  this.store.select(selectNetWorthError);
Math: any;

  ngOnInit(): void {

  const now = new Date();

  const year = now.getFullYear();
  const month = now.getMonth() + 1;

  this.store.dispatch(
    loadDashboard({
      year,
      month
    })
  );

  this.store.dispatch(
    loadCategorySpending({
      year,
      month
    })
  );

  this.store.dispatch(
    loadMonthlyCashFlow({
      year
    })
  );
    this.store.dispatch(
    loadLoanDebt()
  );
    this.store.dispatch(
    loadAccounts()
  );
   this.store.dispatch(
    loadSavingsInvestments({
      year,
      month
    })
  );
  this.categorySpending$
    .subscribe(categories => {

      if (categories.length > 0) {

        this.updateCategoryChart(
          categories
        );

      }

    });

    this.store.dispatch(
  loadNetWorth()
);
}



categorySpendingLoading$ =
  this.store.select(
    selectCategorySpendingLoading
  );

categorySpendingError$ =
  this.store.select(
    selectCategorySpendingError
  );

  monthlyCashFlow$ =
  this.store.select(
    selectMonthlyCashFlow
  );

monthlyCashFlowLoading$ =
  this.store.select(
    selectMonthlyCashFlowLoading
  );

monthlyCashFlowError$ =
  this.store.select(
    selectMonthlyCashFlowError
  );

  calculateBudgetPercentage(
  spent: number,
  budget: number
): number {
  if (budget <= 0) {
    return 0;
  }

  return Math.min(
    (spent / budget) * 100,
    100
  );
}
getCategoryPercentage(
  amount: number,
  categories: any[]
): number {

  const total = categories.reduce(
    (sum, category) => sum + category.amount,
    0
  );

  if (total === 0) {
    return 0;
  }

  return Math.round(
    (amount / total) * 100
  );
}

doughnutChartType = 'doughnut' as const;

doughnutChartData:
  ChartData<'doughnut'> = {
    labels: [],
    datasets: [
      {
        data: [],
        backgroundColor: [
          '#2563eb',
          '#16a34a',
          '#f59e0b',
          '#7c3aed',
          '#0891b2',
          '#f43f5e',
          '#94a3b8'
        ],
        borderWidth: 2,
        borderColor: '#ffffff',
        hoverOffset: 4
      }
    ]
  };


doughnutChartOptions:
  ChartOptions<'doughnut'> = {

    responsive: true,

    maintainAspectRatio: false,

    cutout: '58%',

    plugins: {

      legend: {
        display: false
      },

      tooltip: {
        callbacks: {

          label: (context) => {

            const value =
              context.parsed;

            return ` ₹${value.toLocaleString('en-IN')}`;

          }

        }
      }

    }
  };
  getTotalCategoryAmount(
  categories: any[]
): number {

  return categories.reduce(
    (total, category) =>
      total + category.amount,
    0
  );

}

getCategoryColor(index: number): string {

  const colors = [
    '#2563eb',
    '#16a34a',
    '#f59e0b',
    '#7c3aed',
    '#0891b2',
    '#f43f5e',
    '#94a3b8'
  ];

  return colors[
    index % colors.length
  ];

}
private updateCategoryChart(
  categories: any[]
): void {

  this.doughnutChartData = {

    labels: categories.map(
      category => category.categoryName
    ),

    datasets: [
      {
        data: categories.map(
          category => category.amount
        ),

        backgroundColor: categories.map(
          (_, index) =>
            this.getCategoryColor(index)
        ),

        borderWidth: 2,

        borderColor: '#ffffff',

        hoverOffset: 4
      }
    ]

  };

}
getLastSixMonths(
  cashFlow: MonthlyCashFlow[]
): MonthlyCashFlow[] {

  const now = new Date();

  const lastSixMonths: MonthlyCashFlow[] = [];

  for (let i = 5; i >= 0; i--) {

    const date = new Date(
      now.getFullYear(),
      now.getMonth() - i,
      1
    );

    const year = date.getFullYear();
    const month = date.getMonth() + 1;

    const existingData = cashFlow.find(
      item =>
        item.year === year &&
        item.month === month
    );

    lastSixMonths.push({
      year,
      month,
      monthName: date.toLocaleString('en-US', {
        month: 'short'
      }),

      income: existingData?.income ?? 0,
      expenses: existingData?.expenses ?? 0,
      netCashFlow: existingData?.netCashFlow ?? 0
    });
  }

  return lastSixMonths;
}
}