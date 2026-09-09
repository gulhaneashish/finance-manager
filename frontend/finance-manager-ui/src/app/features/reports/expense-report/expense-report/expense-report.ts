import { Component, inject } from '@angular/core';

import {
  AsyncPipe,
  DecimalPipe
} from '@angular/common';

import { Store } from '@ngrx/store';

import { BaseChartDirective } from 'ng2-charts';

import {
  Chart,
  ArcElement,
  Tooltip,
  Legend
} from 'chart.js';

import type {
  ChartConfiguration,
  ChartData
} from 'chart.js';

import * as ReportActions
  from '../../../../store/report/report.actions';

import {
  selectReportLoading,
  selectReportError,
  selectTotalIncome,
  selectTotalExpense,
  selectNetAmount,
  selectCategoryExpenses
} from '../../../../store/report/report.selectors';

Chart.register(
  ArcElement,
  Tooltip,
  Legend
);

@Component({
  selector: 'app-expense-report',
  standalone: true,
  imports: [
    AsyncPipe,
    DecimalPipe,
    BaseChartDirective
  ],
  templateUrl: './expense-report.html',
  styleUrl: './expense-report.css'
})
export class ExpenseReport {

  private store = inject(Store);

  loading$ =
    this.store.select(selectReportLoading);

  error$ =
    this.store.select(selectReportError);

  totalIncome$ =
    this.store.select(selectTotalIncome);

  totalExpense$ =
    this.store.select(selectTotalExpense);

  netAmount$ =
    this.store.select(selectNetAmount);

  categoryExpenses$ =
    this.store.select(selectCategoryExpenses);

  fromDate = '';

  toDate = '';

  doughnutChartType: 'doughnut' = 'doughnut';

  doughnutChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [
      {
        data: []
      }
    ]
  };

  doughnutChartOptions:
    ChartConfiguration<'doughnut'>['options'] = {

    responsive: true,

    maintainAspectRatio: false,

    plugins: {
      legend: {
        position: 'bottom'
      }
    }

  };

  constructor() {

    this.loadReport();

    this.categoryExpenses$
      .subscribe(categories => {

        this.doughnutChartData = {

          labels: categories.map(
            category =>
              category.categoryName
          ),

          datasets: [
            {
              data: categories.map(
                category =>
                  category.amount
              )
            }
          ]

        };

      });

  }

  loadReport(): void {

    this.store.dispatch(
      ReportActions.loadReport({
        fromDate:
          this.fromDate || undefined,

        toDate:
          this.toDate || undefined
      })
    );

  }

  applyFilter(): void {

    this.loadReport();

  }

  clearFilter(): void {

    this.fromDate = '';

    this.toDate = '';

    this.loadReport();

  }

}