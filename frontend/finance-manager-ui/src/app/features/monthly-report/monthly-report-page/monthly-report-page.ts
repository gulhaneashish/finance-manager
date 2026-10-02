import { Component, inject } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Store } from '@ngrx/store';

import { loadMonthlyReport } from '../../../store/monthly-report/monthly-report.actions';

import {
  selectMonthlyReport,
  selectMonthlyReportLoading,
  selectMonthlyReportError,
  selectMonthlyReportIncome,
  selectMonthlyReportExpenses,
  selectMonthlyReportNetCashFlow,
  selectMonthlyReportExpenseBudget,
  selectMonthlyReportBudgetSpent,
  selectMonthlyReportBudgetRemaining,
  selectMonthlyReportSavingsTarget,
  selectMonthlyReportSavingsAmount,
  selectMonthlyReportInvestmentTarget,
  selectMonthlyReportInvestmentAmount,
  selectMonthlyReportTotalBorrowed,
  selectMonthlyReportTotalLent,
  selectMonthlyReportCategories
} from '../../../store/monthly-report/monthly-report.selectors';
import { MatIconModule } from "@angular/material/icon";

@Component({
  selector: 'app-monthly-report-page',
  standalone: true,
  imports: [
    AsyncPipe,
    DecimalPipe,
    MatIconModule
],
  templateUrl: './monthly-report-page.html',
  styleUrl: './monthly-report-page.css'
})
export class MonthlyReportPage {

  private store = inject(Store);

  report$ =
    this.store.select(selectMonthlyReport);

  loading$ =
    this.store.select(selectMonthlyReportLoading);

  error$ =
    this.store.select(selectMonthlyReportError);

  income$ =
    this.store.select(selectMonthlyReportIncome);

  expenses$ =
    this.store.select(selectMonthlyReportExpenses);

  netCashFlow$ =
    this.store.select(selectMonthlyReportNetCashFlow);

  expenseBudget$ =
    this.store.select(selectMonthlyReportExpenseBudget);

  budgetSpent$ =
    this.store.select(selectMonthlyReportBudgetSpent);

  budgetRemaining$ =
    this.store.select(selectMonthlyReportBudgetRemaining);

  savingsTarget$ =
    this.store.select(selectMonthlyReportSavingsTarget);

  savingsAmount$ =
    this.store.select(selectMonthlyReportSavingsAmount);

  investmentTarget$ =
    this.store.select(selectMonthlyReportInvestmentTarget);

  investmentAmount$ =
    this.store.select(selectMonthlyReportInvestmentAmount);

  totalBorrowed$ =
    this.store.select(selectMonthlyReportTotalBorrowed);

  totalLent$ =
    this.store.select(selectMonthlyReportTotalLent);

  categories$ =
    this.store.select(selectMonthlyReportCategories);

  selectedYear = new Date().getFullYear();

  selectedMonth = new Date().getMonth() + 1;

  constructor() {
    this.loadReport();
  }

  loadReport(): void {
    this.store.dispatch(
      loadMonthlyReport({
        year: this.selectedYear,
        month: this.selectedMonth
      })
    );
  }

  previousMonth(): void {

    this.selectedMonth--;

    if (this.selectedMonth === 0) {
      this.selectedMonth = 12;
      this.selectedYear--;
    }

    this.loadReport();
  }

  nextMonth(): void {

    this.selectedMonth++;

    if (this.selectedMonth === 13) {
      this.selectedMonth = 1;
      this.selectedYear++;
    }

    this.loadReport();
  }
}