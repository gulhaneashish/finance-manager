import { Component, inject } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Store } from '@ngrx/store';

import * as MonthlyReportActions
  from '../../../store/monthly-report/monthly-report.actions';

import {
  selectMonthlyReportLoading,
  selectMonthlyReportError,
  selectMonthlyReport
} from '../../../store/monthly-report/monthly-report.selectors';

@Component({
  selector: 'app-monthly-finance-page',
  standalone: true,
  imports: [
    AsyncPipe,
    
  ],
  templateUrl: './monthly-finance-page.html',
  styleUrl: './monthly-finance-page.css'
})
export class MonthlyFinancePage {

  private store = inject(Store);

  // -------------------------
  // NgRx
  // -------------------------

  loading$ = this.store.select(
    selectMonthlyReportLoading
  );

  error$ = this.store.select(
    selectMonthlyReportError
  );

  report$ = this.store.select(
    selectMonthlyReport
  );


  // -------------------------
  // Selected Month
  // -------------------------

  selectedYear = new Date().getFullYear();

  selectedMonth = new Date().getMonth() + 1;


  // -------------------------
  // Constructor
  // -------------------------

  constructor() {
    this.loadReport();
  }


  // -------------------------
  // Load Report
  // -------------------------

  loadReport(): void {

    this.store.dispatch(
      MonthlyReportActions.loadMonthlyReport({
        year: this.selectedYear,
        month: this.selectedMonth
      })
    );

  }


  // -------------------------
  // Previous Month
  // -------------------------

  previousMonth(): void {

    this.selectedMonth--;

    if (this.selectedMonth === 0) {

      this.selectedMonth = 12;
      this.selectedYear--;

    }

    this.loadReport();

  }


  // -------------------------
  // Next Month
  // -------------------------

  nextMonth(): void {

    this.selectedMonth++;

    if (this.selectedMonth === 13) {

      this.selectedMonth = 1;
      this.selectedYear++;

    }

    this.loadReport();

  }


  // -------------------------
  // Month Name
  // -------------------------

  getMonthName(): string {

    const date = new Date(
      this.selectedYear,
      this.selectedMonth - 1,
      1
    );

    return date.toLocaleString(
      'default',
      {
        month: 'long'
      }
    );

  }

}