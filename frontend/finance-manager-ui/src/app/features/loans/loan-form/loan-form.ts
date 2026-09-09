import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject
} from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import * as AccountActions
  from '../../../store/accounts/accounts.actions';

import {
  AsyncPipe,
  CommonModule,
  DecimalPipe
} from '@angular/common';

import { Store } from '@ngrx/store';

import {
  createLoan,
  updateLoan
} from '../../../store/loans/loan.actions';

import {
  selectLoanLoading
} from '../../../store/loans/loan.selectors';

import {
  Loan,
  LoanCreate,
  LoanType
} from '../../../core/models/loan.model';

import {
  selectActiveAccounts
} from '../../../store/accounts/accounts.selectors';

import { map } from 'rxjs';

@Component({
  selector: 'app-loan-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AsyncPipe,
    DecimalPipe
  ],
  templateUrl: './loan-form.html',
  styleUrl: './loan-form.css'
})
export class LoanForm implements OnInit {

  private fb = inject(FormBuilder);

  private store = inject(Store);

  @Input() loan: Loan | null = null;

  @Output() formClosed =
    new EventEmitter<void>();

  loading$ =
    this.store.select(selectLoanLoading);

  accounts$ =
    this.store.select(selectActiveAccounts);

  LoanType = LoanType;

  Accounts$ = this.accounts$.pipe(
    map(accounts =>
      accounts.filter(account =>
        account.isActive &&
        account.accountType !== 'CREDIT_CARD'
      )
    )
  );

  loanForm =
    this.fb.nonNullable.group({

      personName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],

      type: [
        LoanType.Borrowed,
        Validators.required
      ],

      originalAmount: [
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

      loanDate: [
        this.getToday()
      ],

      dueDate: [
        ''
      ],

      notes: [
        ''
      ]
    });

  ngOnInit(): void {

    this.store.dispatch(
      AccountActions.loadActiveAccounts()
    );

    if (!this.loan) {
      return;
    }

    this.loanForm.patchValue({

      personName:
        this.loan.personName,

      type:
        this.loan.type,

      originalAmount:
        this.loan.originalAmount,

      accountId:
        this.loan.accountId,

      loanDate:
        this.loan.loanDate.substring(0, 10),

      dueDate:
        this.loan.dueDate
          ? this.loan.dueDate.substring(0, 10)
          : '',

      notes:
        this.loan.notes ?? ''
    });
  }

  submit(): void {

    if (this.loanForm.invalid) {

      this.loanForm.markAllAsTouched();

      return;
    }

    const value =
      this.loanForm.getRawValue();

    // =========================================================
    // UPDATE
    // =========================================================

    if (this.loan) {

      this.store.dispatch(
        updateLoan({

          loanId:
            this.loan.id,

          loan: {

            personName:
              value.personName,

            type:
              value.type,

            originalAmount:
              value.originalAmount,

            accountId:
              value.accountId,

            loanDate:
              value.loanDate,

            dueDate:
              value.dueDate || null,

            notes:
              value.notes || null
          }
        })
      );

      this.formClosed.emit();

      return;
    }

    // =========================================================
    // CREATE
    // =========================================================

    const loan: LoanCreate = {

      personName:
        value.personName,

      type:
        value.type,

      originalAmount:
        value.originalAmount,

      accountId:
        value.accountId,

      loanDate:
        value.loanDate,

      dueDate:
        value.dueDate || null,

      notes:
        value.notes || null
    };

    this.store.dispatch(
      createLoan({ loan })
    );

    this.formClosed.emit();
  }

  close(): void {
    this.formClosed.emit();
  }

  private getToday(): string {

    return new Date()
      .toISOString()
      .substring(0, 10);
  }
}