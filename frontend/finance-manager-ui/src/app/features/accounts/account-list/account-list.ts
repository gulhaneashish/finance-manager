import {
  Component,
  EventEmitter,
  inject,
  OnInit,
  Output
} from '@angular/core';

import { CreditCardPurchaseForm }
  from '../../credit-card/credit-card-purchase-form/credit-card-purchase-form';
import { CreditCardPaymentForm }
  from '../../credit-card/credit-card-payment-form/credit-card-payment-form';
import { AccountForm }
  from '../account-form/account-form';

import {
  AsyncPipe,
  DecimalPipe
} from '@angular/common';

import { Store } from '@ngrx/store';

import {
  deleteAccount,
  loadAccounts,
  loadActiveAccounts
} from '../../../store/accounts/accounts.actions';

import {
  selectAccounts,
  selectAccountsLoading,
  selectAccountsError
} from '../../../store/accounts/accounts.selectors';

import { Account }
  from '../../../core/models/account.model';


@Component({
  selector: 'app-account-list',

  standalone: true,

  imports: [
    AccountForm,
    AsyncPipe,
    DecimalPipe,
    CreditCardPurchaseForm,
    CreditCardPaymentForm
  ],

  templateUrl: './account-list.html',

  styleUrl: './account-list.css'
})
export class AccountList implements OnInit {

  private store = inject(Store);

  accounts$ =
    this.store.select(selectAccounts);

  loading$ =
    this.store.select(selectAccountsLoading);

  error$ =
    this.store.select(selectAccountsError);
    
showPaymentForm = false;

selectedCreditCardId: number | null = null;
  @Output()
  saved = new EventEmitter<void>();

  @Output()
  cancelled = new EventEmitter<void>();


  showForm = false;

  selectedAccount: Account | null = null;


  showPurchaseForm = false;


  ngOnInit(): void {

    this.store.dispatch(
      loadAccounts()
    );

      this.store.dispatch(
      loadActiveAccounts()
    );

  }


  openAddForm(): void {

    this.selectedAccount = null;

    this.showForm = true;

  }


  editAccount(account: Account): void {

    this.selectedAccount = account;

    this.showForm = true;

  }


  closeForm(): void {

    this.showForm = false;

    this.selectedAccount = null;

  }


  deleteAccount(id: number): void {

    const confirmed =
      confirm(
        'Are you sure you want to delete this account?'
      );

    if (!confirmed) {
      return;
    }

    this.store.dispatch(
      deleteAccount({ id })
    );

  }


  openPurchaseForm(accountId: number): void {

    this.selectedCreditCardId =
      accountId;

    this.showPurchaseForm = true;

  }


  closePurchaseForm(): void {

    this.showPurchaseForm = false;

    this.selectedCreditCardId = null;

  }
openPaymentForm(accountId: number): void {

  this.selectedCreditCardId = accountId;

  this.showPaymentForm = true;
}
closePaymentForm(): void {

  this.showPaymentForm = false;

  this.selectedCreditCardId = null;
}
}