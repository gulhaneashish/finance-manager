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
import { FormsModule } from '@angular/forms';
import {
  CategorySpending,
  DashboardFilter,
  DashboardPeriod,
  MonthlyCashFlow
} from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    AsyncPipe,
    DecimalPipe,
    FormsModule,
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
  LoanType = LoanType;

  selectedPeriod: DashboardPeriod = 'this_month';
  customStartDate: string = '';
  customEndDate: string = '';

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

  private formatDateInput(d: Date): string {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  setPeriod(period: DashboardPeriod): void {
    this.selectedPeriod = period;

    if (period === 'custom') {
      if (!this.customStartDate || !this.customEndDate) {
        const now = new Date();
        this.customEndDate = this.formatDateInput(now);
        this.customStartDate = this.formatDateInput(new Date(now.getFullYear(), now.getMonth(), 1));
      }
      this.applyCustomRange();
      return;
    }

    const now = new Date();
    const year = now.getFullYear();

    this.store.dispatch(
      loadDashboard({
        filter: { period }
      })
    );

    this.store.dispatch(
      loadCategorySpending({
        filter: { period }
      })
    );

    this.store.dispatch(
      loadSavingsInvestments({
        filter: { period }
      })
    );

    this.store.dispatch(
      loadMonthlyCashFlow({
        year
      })
    );
  }

  applyCustomRange(): void {
    if (!this.customStartDate || !this.customEndDate) {
      return;
    }

    const year = new Date(this.customStartDate).getFullYear() || new Date().getFullYear();

    this.store.dispatch(
      loadDashboard({
        filter: {
          period: 'custom',
          startDate: this.customStartDate,
          endDate: this.customEndDate
        }
      })
    );

    this.store.dispatch(
      loadCategorySpending({
        filter: {
          period: 'custom',
          startDate: this.customStartDate,
          endDate: this.customEndDate
        }
      })
    );

    this.store.dispatch(
      loadSavingsInvestments({
        filter: {
          period: 'custom',
          startDate: this.customStartDate,
          endDate: this.customEndDate
        }
      })
    );

    this.store.dispatch(
      loadMonthlyCashFlow({
        year
      })
    );
  }

  ngOnInit(): void {
    const now = new Date();
    this.customEndDate = this.formatDateInput(now);
    this.customStartDate = this.formatDateInput(new Date(now.getFullYear(), now.getMonth(), 1));

    // Default to 'this_month'
    this.setPeriod('this_month');

    this.store.dispatch(loadLoanDebt());
    this.store.dispatch(loadAccounts());
    this.store.dispatch(loadNetWorth());

    this.categorySpending$
      .subscribe(categories => {
        if (categories && categories.length > 0) {
          this.updateCategoryChart(categories);
        } else {
          this.clearCategoryChart();
        }
      });
  }

  private clearCategoryChart(): void {
    this.doughnutChartData = {
      labels: [],
      datasets: [
        {
          data: [],
          backgroundColor: [],
          borderWidth: 2,
          borderColor: '#ffffff',
          hoverOffset: 4
        }
      ]
    };
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