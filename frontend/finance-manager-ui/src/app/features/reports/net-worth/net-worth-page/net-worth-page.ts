import { Component, inject } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { Store } from '@ngrx/store';

import { loadNetWorth } from '../../../../store/net-worth/net-worth.actions';

import {
  selectNetWorthLoading,
  selectNetWorthError,
  selectTotalAssets,
  selectBankBalance,
  selectCashBalance,
  selectSavingsBalance,
  selectInvestmentBalance,
  selectCreditCardDebt,
  selectLoansPayable,
  selectLoansReceivable,
  selectTotalLiabilities,
  selectNetWorthAmount
} from '../../../../store/net-worth/net-worth.selectors';

@Component({
  selector: 'app-net-worth-page',
  standalone: true,
  imports: [
    AsyncPipe,
    DecimalPipe
  ],
  templateUrl: './net-worth-page.html',
  styleUrl: './net-worth-page.css'
})
export class NetWorthPage {

  private store = inject(Store);

  loading$ =
    this.store.select(selectNetWorthLoading);

  error$ =
    this.store.select(selectNetWorthError);

  totalAssets$ =
    this.store.select(selectTotalAssets);

  bankBalance$ =
    this.store.select(selectBankBalance);

  cashBalance$ =
    this.store.select(selectCashBalance);

  savingsBalance$ =
    this.store.select(selectSavingsBalance);

  investmentBalance$ =
    this.store.select(selectInvestmentBalance);

  creditCardDebt$ =
    this.store.select(selectCreditCardDebt);

  loansPayable$ =
    this.store.select(selectLoansPayable);

  loansReceivable$ =
    this.store.select(selectLoansReceivable);

  totalLiabilities$ =
    this.store.select(selectTotalLiabilities);

  netWorth$ =
    this.store.select(selectNetWorthAmount);

  constructor() {
    this.store.dispatch(loadNetWorth());
  }
}