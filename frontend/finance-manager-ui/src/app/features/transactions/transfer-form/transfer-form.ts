import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  OnInit
} from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { AsyncPipe } from '@angular/common';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import {
  selectActiveAccounts
} from '../../../store/accounts/accounts.selectors';

import {
  loadAccounts,
  loadActiveAccounts
} from '../../../store/accounts/accounts.actions';

import {
  createTransfer
} from '../../../store/transactions/transaction.actions';
import { TransactionPurpose } from '../../../core/models/transaction.model';

import { CommonModule, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-transfer-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AsyncPipe,
    DecimalPipe
  ],
  templateUrl: './transfer-form.html',
  styleUrl: './transfer-form.css'
})
export class TransferForm implements OnInit {
  @Output() formClosed = new EventEmitter<void>();

  @Input() set initialData(data: { toAccountId?: number; amount?: number | null; description?: string | null } | null | undefined) {
    if (data) {
      if (data.toAccountId) {
        this.transferForm.patchValue({ toAccountId: data.toAccountId });
      }
      if (data.amount && data.amount > 0) {
        this.transferForm.patchValue({ amount: data.amount });
      }
      if (data.description) {
        this.transferForm.patchValue({ description: data.description });
      }
    }
  }

  private fb = inject(FormBuilder);
  private store = inject(Store);

  accounts$ = this.store.select(
    selectActiveAccounts
  );

 transferAccounts$ = this.accounts$.pipe(
  map(accounts =>
    accounts.filter(
      account =>
        account.isActive &&
        account.accountType !== 'CREDIT_CARD'
    )
  )
);

  transferForm =
    this.fb.nonNullable.group({

      fromAccountId: [
        0,
        Validators.required
      ],

      toAccountId: [
        0,
        Validators.required
      ],

      transactionPurpose: [
        TransactionPurpose.Expense,
        Validators.required
      ],

      amount: [
        0,
        [
          Validators.required,
          Validators.min(0.01)
        ]
      ],

      transactionDate: [
        new Date()
          .toISOString()
          .slice(0, 10),
        Validators.required
      ],

      description: [
        ''
      ]
    });

ngOnInit(): void {
  this.store.dispatch(
    loadActiveAccounts()
  );

}

  submit(): void {

    if (this.transferForm.invalid) {
      this.transferForm.markAllAsTouched();
      return;
    }

    const value =
      this.transferForm.getRawValue();

    if (
      value.fromAccountId ===
      value.toAccountId
    ) {
      alert(
        'Source and destination accounts must be different.'
      );
      return;
    }

    this.store.dispatch(
      createTransfer({
        transfer: {

          fromAccountId:
            value.fromAccountId,

          toAccountId:
            value.toAccountId,

          amount:
            value.amount,

          transactionPurpose:
            value.transactionPurpose,

          transactionDate:
            `${value.transactionDate}T00:00:00`,

          description:
            value.description

        }
      })
    );

    this.formClosed.emit();
  }

  cancel(): void {
    this.formClosed.emit();
  }
}