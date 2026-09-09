import {
  Component,
  inject,
  Input,
  OnChanges
} from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Output, EventEmitter } from '@angular/core';
import { Store } from '@ngrx/store';

import {
  createAccount,
  updateAccount
} from '../../../store/accounts/accounts.actions';

import { Account } from '../../../core/models/account.model';

@Component({
  selector: 'app-account-form',
  standalone: true,
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './account-form.html',
  styleUrl: './account-form.css'
})
export class AccountForm implements OnChanges {

  private fb = inject(FormBuilder);

  private store = inject(Store);

  @Input() account: Account | null = null;

  @Output() formClosed = new EventEmitter<void>();

  accountForm = this.fb.group({

    name: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],

    accountType: [
      '',
      Validators.required
    ],

    openingBalance: [
      0,
      [
        Validators.required,
        Validators.min(0)
      ]
    ],

    creditLimit: [
  null as number | null,
  Validators.min(0.01)
]

  });

  isSubmitting = false;

  errorMessage = '';

  accountTypes = [
    'BANK',
    'CASH',
    'SAVINGS',
    'INVESTMENT',
    'CREDIT_CARD'
  ];

  get isCreditCard(): boolean {

    return this.accountForm.get(
      'accountType'
    )?.value === 'CREDIT_CARD';

  }

  submit(): void {

    if (this.accountForm.invalid) {

      this.accountForm.markAllAsTouched();

      return;
    }

    const formValue =
      this.accountForm.getRawValue();

    const request = {

      name: formValue.name!,

      accountType:
        formValue.accountType!,

      openingBalance:
        formValue.openingBalance ?? 0,

      creditLimit:
        this.isCreditCard
          ? formValue.creditLimit
          : null,

      isActive:
        this.account?.isActive ?? true

    };

    if (this.account) {

      this.store.dispatch(
        updateAccount({
          id: this.account.id,
          account: request
        })
      );

    } else {

      this.store.dispatch(
        createAccount({
          account: request
        })
      );

    }
this.formClosed.emit();
  }

  ngOnChanges(): void {

    if (this.account) {

      this.accountForm.patchValue({

        name: this.account.name,

        accountType:
          this.account.accountType,

        openingBalance:
          this.account.openingBalance,

        creditLimit:
          this.account.creditLimit

      });

    }

  }

}