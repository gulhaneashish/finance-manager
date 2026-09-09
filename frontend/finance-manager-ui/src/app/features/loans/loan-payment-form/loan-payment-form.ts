import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { AsyncPipe, CommonModule, DecimalPipe } from '@angular/common';
import { Store } from '@ngrx/store';
import * as AccountActions from '../../../store/accounts/accounts.actions'
import { addLoanPayment } from '../../../store/loans/loan.actions';
import {  selectActiveAccounts } from '../../../store/accounts/accounts.selectors';

@Component({
  selector: 'app-loan-payment-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AsyncPipe,
    DecimalPipe
  ],
  templateUrl: './loan-payment-form.html',
  styleUrl: './loan-payment-form.css'
})
export class LoanPaymentForm implements OnInit{

  private fb = inject(FormBuilder);
  private store = inject(Store);

  @Input({ required: true })
  loanId!: number;

  @Output()
  formClosed = new EventEmitter<void>();

  accounts$ = this.store.select(selectActiveAccounts);
ngOnInit(): void {
    this.store.dispatch(
      AccountActions.loadActiveAccounts()
    );
}
  paymentForm = this.fb.nonNullable.group({
    amount: [
      0,
      [
        Validators.required,
        Validators.min(0.01)
      ]
    ],

    accountId: [
      0,
      [
        Validators.required,
        Validators.min(1)
      ]
    ],

    paymentDate: [
      new Date()
        .toISOString()
        .slice(0, 10),
      Validators.required
    ],

    notes: [
      ''
    ]
  });

  submit(): void {

    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const value = this.paymentForm.getRawValue();

    this.store.dispatch(
      addLoanPayment({
        loanId: this.loanId,
        payment: {
          amount: value.amount,
          accountId: value.accountId,
          paymentDate: `${value.paymentDate}T00:00:00`,
          notes: value.notes
        }
      })
    );

    this.formClosed.emit();
  }

  close(): void {
    this.formClosed.emit();
  }
}