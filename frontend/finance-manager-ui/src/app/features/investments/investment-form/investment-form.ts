import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

import { Account } from '../../../core/models/account.model';
import { InvestmentType } from '../../../core/models/investment.model';
import { Subject,  } from 'rxjs';
import { filter, map, takeUntil } from 'rxjs/operators';
import * as InvestmentActions
  from '../../../store/investment/investment.actions';

import * as AccountActions
  from '../../../store/accounts/accounts.actions';

import {
  selectAccounts,
  selectActiveAccounts
} from '../../../store/accounts/accounts.selectors';
import { selectInvestmentOperationSuccess } from '../../../store/investment/investment.selectors';

@Component({
  selector: 'app-investment-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './investment-form.html',
  styleUrl: './investment-form.css'
})
export class InvestmentForm implements OnInit,OnDestroy {

  private fb = inject(FormBuilder);
  private store = inject(Store);
  private router = inject(Router);
private destroy$ = new Subject<void>();
  investmentTypes = Object.values(InvestmentType);

  accounts$: Observable<Account[]> =
    this.store.select(selectActiveAccounts);

  investmentForm = this.fb.group({
    accountId: [
      0,
      [
        Validators.required,
        Validators.min(1)
      ]
    ],

    name: [
      '',
      [
        Validators.required,
        Validators.minLength(2)
      ]
    ],

    investmentType: [
      InvestmentType.MutualFund,
      Validators.required
    ],

    amount: [
      0,
      [
        Validators.required,
        Validators.min(0.01)
      ]
    ],

    investmentDate: [
      new Date().toISOString().substring(0, 10),
      Validators.required
    ],

    description: ['']
  });
  
transferAccounts$ = this.accounts$.pipe(
  map((accounts: any[]) =>
    accounts.filter(
      account =>
        account.isActive &&
        account.accountType !== 'CREDIT_CARD'
    )
  )
);
ngOnInit(): void {

  this.store.dispatch(
    AccountActions.loadActiveAccounts()
  );

  this.store.select(
    selectInvestmentOperationSuccess
  )
  .pipe(
    filter(message => !!message),
    takeUntil(this.destroy$)
  )
  .subscribe(() => {
    this.router.navigate(['/investments']);
  });
}
ngOnDestroy(): void {
  this.destroy$.next();
  this.destroy$.complete();
}
  submit(): void {

    if (this.investmentForm.invalid) {
      this.investmentForm.markAllAsTouched();
      return;
    }

    const formValue =
      this.investmentForm.getRawValue();

    this.store.dispatch(
      InvestmentActions.createInvestment({
        accountId: formValue.accountId!,
        name: formValue.name!,
        investmentType: formValue.investmentType!,
        amount: formValue.amount!,
        investmentDate: formValue.investmentDate!,
        description:
          formValue.description || undefined
      })
    );

  }

  cancel(): void {
    this.router.navigate(['/investments']);
  }
}