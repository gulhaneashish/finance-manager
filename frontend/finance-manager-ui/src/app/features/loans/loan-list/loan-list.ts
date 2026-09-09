import { Component, inject, OnDestroy } from '@angular/core';
import {
  AsyncPipe,
  CommonModule,
  DatePipe,
  DecimalPipe
} from '@angular/common';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { loadLoans } from '../../../store/loans/loan.actions';
import {
  selectLoanSuccessMessage
} from '../../../store/loans/loan.selectors';
import {
  selectLoans,
  selectLoanLoading,
  selectLoanError
} from '../../../store/loans/loan.selectors';
import { LoanPaymentForm }
  from '../loan-payment-form/loan-payment-form';
import { Loan, LoanType } from '../../../core/models/loan.model';

import { LoanForm } from '../loan-form/loan-form';

@Component({
  selector: 'app-loan-list',
  standalone: true,
  imports: [
    CommonModule,
    AsyncPipe,
    DecimalPipe,
    DatePipe,
    LoanForm,
    LoanPaymentForm
  ],
  templateUrl: './loan-list.html',
  styleUrl: './loan-list.css'
})
export class LoanList implements OnDestroy {

  private store = inject(Store);
  private successSubscription?: Subscription;

  
  loans$ =
    this.store.select(selectLoans);

  loading$ =
    this.store.select(selectLoanLoading);

  error$ =
    this.store.select(selectLoanError);
  successMessage$ =
  this.store.select(
    selectLoanSuccessMessage
  );
  LoanType = LoanType;

  showForm = false;

constructor() {

  this.store.dispatch(loadLoans());

 this.successSubscription =
  this.successMessage$.subscribe(message => {
    if (message && (this.showForm || this.selectedLoan !== null)) {
      this.closeAllForms();
    }
  });

}

ngOnDestroy(): void {
  this.successSubscription?.unsubscribe();
}
 openAddForm(): void {
  this.selectedLoan = null;
  this.showForm = true;
}


  
selectedLoan: Loan | null = null;



showPaymentForm = false;
selectedLoanId: number | null = null;

openPaymentForm(loanId: number): void {
  this.selectedLoanId = loanId;
  this.showPaymentForm = true;
}

closePaymentForm(): void {
  this.showPaymentForm = false;
  this.selectedLoanId = null;
}


openEditForm(loan: Loan): void {
  this.showForm = false;
  this.selectedLoan = loan;
}

closeAllForms(): void {
  this.showForm = false;
  this.selectedLoan = null;
}
}
