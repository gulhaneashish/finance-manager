import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject
} from '@angular/core';

import {
  AsyncPipe
} from '@angular/common';

import {
  FormsModule
} from '@angular/forms';

import {
  Store
} from '@ngrx/store';

import {
  makePayment
} from '../../../store/credit-card/credit-card.actions';

import {
  selectCreditCardLoading,
  selectCreditCardError,
  selectCreditCardSuccessMessage
} from '../../../store/credit-card/credit-card.selectors';

import {
  loadAccounts
} from '../../../store/accounts/accounts.actions';

import {
  selectAccounts
} from '../../../store/accounts/accounts.selectors';

@Component({
  selector: 'app-credit-card-payment-form',

  standalone: true,

  imports: [
    FormsModule,
    AsyncPipe
  ],

  templateUrl:
    './credit-card-payment-form.html',

  styleUrl:
    './credit-card-payment-form.css'
})
export class CreditCardPaymentForm
  implements OnInit {

  private store = inject(Store);


  @Input()
  creditCardAccountId!: number;


  @Output()
  formClosed =
    new EventEmitter<void>();


  loading$ =
    this.store.select(
      selectCreditCardLoading
    );

  error$ =
    this.store.select(
      selectCreditCardError
    );

  successMessage$ =
    this.store.select(
      selectCreditCardSuccessMessage
    );

  accounts$ =
    this.store.select(
      selectAccounts
    );


  fromAccountId: number | null = null;

  amount: number | null = null;

  description = '';

  paymentDate = '';


  ngOnInit(): void {

    this.store.dispatch(
      loadAccounts()
    );

    this.paymentDate =
      this.getTodayDate();

  }


  submit(): void {

    if (
      !this.fromAccountId ||
      !this.amount ||
      this.amount <= 0 ||
      !this.paymentDate
    ) {
      return;
    }


    if (
      this.fromAccountId ===
      this.creditCardAccountId
    ) {
      return;
    }


    this.store.dispatch(
      makePayment({
        payment: {

          fromAccountId:
            this.fromAccountId,

          creditCardAccountId:
            this.creditCardAccountId,

          amount:
            this.amount,

          description:
            this.description,

          paymentDate:
            this.paymentDate

        }
      })
    );

  }


  close(): void {

    this.formClosed.emit();

  }


  private getTodayDate(): string {

    const today =
      new Date();

    return today
      .toISOString()
      .split('T')[0];

  }

}