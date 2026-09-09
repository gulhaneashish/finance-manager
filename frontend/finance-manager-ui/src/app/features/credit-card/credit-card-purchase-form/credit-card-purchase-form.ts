import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject
} from '@angular/core';

import {
  FormsModule
} from '@angular/forms';

import {
  AsyncPipe
} from '@angular/common';

import { Store } from '@ngrx/store';

import {
  makePurchase
} from '../../../store/credit-card/credit-card.actions';

import {
  selectCreditCardLoading,
  selectCreditCardError,
  selectCreditCardSuccessMessage
} from '../../../store/credit-card/credit-card.selectors';

import {
  loadCategories
} from '../../../store/categories/category.actions';

import {
  selectAllCategories
} from '../../../store/categories/category.selectors';


@Component({
  selector: 'app-credit-card-purchase-form',

  standalone: true,

  imports: [
    FormsModule,
    AsyncPipe
  ],

  templateUrl:
    './credit-card-purchase-form.html',

  styleUrl:
    './credit-card-purchase-form.css'
})
export class CreditCardPurchaseForm
  implements OnInit {

  private store = inject(Store);


  @Input()
  accountId!: number;


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

  categories$ =
    this.store.select(
      selectAllCategories
    );


  categoryId: number | null = null;

  amount: number | null = null;

  description = '';

  transactionDate = '';


  ngOnInit(): void {

    this.store.dispatch(
      loadCategories()
    );

    this.transactionDate =
      this.getTodayDate();

  }


  submit(): void {

    if (
      !this.categoryId ||
      !this.amount ||
      this.amount <= 0 ||
      !this.transactionDate
    ) {
      return;
    }


    this.store.dispatch(
      makePurchase({
        purchase: {

          accountId:
            this.accountId,

          categoryId:
            this.categoryId,

          amount:
            this.amount,

          description:
            this.description,

          transactionDate:
            this.transactionDate

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