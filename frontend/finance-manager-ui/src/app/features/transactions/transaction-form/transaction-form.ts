import { Component, inject, OnInit } from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  AsyncPipe,
  CommonModule,
  DecimalPipe
} from '@angular/common';

import { Store } from '@ngrx/store';
import { TransactionPurpose, TransactionType } from '../../../core/models/transaction.model';
import { map } from 'rxjs';

import {
  selectAccounts,
  selectActiveAccounts
} from '../../../store/accounts/accounts.selectors';

import {
  createTransaction
} from '../../../store/transactions/transaction.actions';

import {
  selectAllCategories
} from '../../../store/categories/category.selectors';
import { loadActiveAccounts } from '../../../store/accounts/accounts.actions';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AsyncPipe,
    DecimalPipe,
    CommonModule
  ],
  templateUrl: './transaction-form.html',
  styleUrl: './transaction-form.css'
})
export class TransactionForm implements OnInit {

  private fb = inject(FormBuilder);
  private store = inject(Store);
TransactionType=TransactionType
  categories$ = this.store.select(
    selectAllCategories
  );

  accounts$ = this.store.select(
    selectActiveAccounts
  );

  ngOnInit(): void {
       this.store.dispatch(
          loadActiveAccounts()
        );
      
  }
  transactionAccounts$ = this.accounts$.pipe(
   
  map(accounts =>
    accounts.filter(
      account =>
        account.isActive &&
        account.accountType !== 'CREDIT_CARD'
    )
  )

  );

  transactionForm =
    this.fb.nonNullable.group({

      accountId: [
        0,
        [
          Validators.required,
          Validators.min(1)
        ]
      ],

      categoryId: [
        null as number | null
      ],

      amount: [
        0,
        [
          Validators.required,
          Validators.min(0.01)
        ]
      ],

      type: [
        TransactionType.Expense,
        Validators.required
      ],

      description: [
        '',
        [
          Validators.maxLength(250)
        ]
      ],

      transactionDate: [
        new Date()
          .toISOString()
          .slice(0, 10),
        Validators.required
      ],

      purpose: [
        TransactionPurpose.Expense,
        Validators.required
      ]

    });

  submit(): void {

    console.log('🔥 Submit clicked');

    console.log(
      'Form valid:',
      this.transactionForm.valid
    );

    console.log(
      'Form value:',
      this.transactionForm.getRawValue()
    );

    if (this.transactionForm.invalid) {

      console.log(
        '❌ Form is invalid'
      );

      this.transactionForm.markAllAsTouched();

      return;
    }

    const value =
      this.transactionForm.getRawValue();

    console.log(
      '🚀 Dispatching createTransaction'
    );

    this.store.dispatch(
      createTransaction({
        transaction: {

          accountId:
            value.accountId,

          categoryId:
            value.categoryId,

          amount:
            value.amount,

          type:
            value.type,

          description:
            value.description,

          transactionDate:
            `${value.transactionDate}T00:00:00`,

          purpose:
            value.purpose

        }
      })
    );
  }
}