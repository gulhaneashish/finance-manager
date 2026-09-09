import {
  Component,
  inject,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import {
  FormBuilder,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';

import { map } from 'rxjs';

import * as InvestmentActions
  from '../../../store/investment/investment.actions';

import {
  selectInvestments,
  selectInvestmentSummary,
  selectInvestmentLoading,
  selectInvestmentError,
  selectInvestmentOperationSuccess
} from '../../../store/investment/investment.selectors';

import {
  selectActiveAccounts
} from '../../../store/accounts/accounts.selectors';

import {
  loadActiveAccounts
} from '../../../store/accounts/accounts.actions';

@Component({
  selector: 'app-investment-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './investment-list.html',
  styleUrl: './investment-list.css'
})
export class InvestmentList implements OnInit {

  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  investments$ = this.store.select(
    selectInvestments
  );

  summary$ = this.store.select(
    selectInvestmentSummary
  );

  loading$ = this.store.select(
    selectInvestmentLoading
  );

  error$ = this.store.select(
    selectInvestmentError
  );

  operationSuccess$ =
    this.store.select(
      selectInvestmentOperationSuccess
    );

  showUpdateForm = false;

  selectedInvestmentId:
    number | null = null;

  showSellForm = false;

  selectedInvestmentValue = 0;

  accounts$ = this.store.select(
    selectActiveAccounts
  );

  transferAccounts$ = this.accounts$.pipe(
    map(accounts =>
      accounts.filter(
        account =>
          account.accountType !== 'CREDIT_CARD'
      )
    )
  );

  updateForm = this.fb.group({

    currentValue: [
      0,
      [
        Validators.required,
        Validators.min(0.01)
      ]
    ]

  });

  sellForm = this.fb.group({

    accountId: [
      0,
      [
        Validators.required,
        Validators.min(1)
      ]
    ],

    sellAmount: [
      0,
      [
        Validators.required,
        Validators.min(0.01)
      ]
    ],

    sellDate: [
      new Date()
        .toISOString()
        .substring(0, 10),
      Validators.required
    ],

    description: ['']

  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {

    this.store.dispatch(
      InvestmentActions.loadInvestments()
    );

    this.store.dispatch(
      InvestmentActions.loadInvestmentSummary()
    );

    this.store.dispatch(
      loadActiveAccounts()
    );

  }

  addInvestment(): void {
    this.router.navigate([
      '/investments/add'
    ]);
  }

  updateValue(
    id: number,
    currentValue: number
  ): void {

    this.selectedInvestmentId = id;

    this.showUpdateForm = true;

    this.updateForm.reset({
      currentValue: currentValue
    });
  }

  submitUpdate(): void {

    if (
      this.updateForm.invalid ||
      this.selectedInvestmentId === null
    ) {
      this.updateForm.markAllAsTouched();
      return;
    }

    const currentValue =
      this.updateForm.get(
        'currentValue'
      )?.value;

    this.store.dispatch(
      InvestmentActions.updateInvestmentValue({
        id: this.selectedInvestmentId,
        currentValue: currentValue!
      })
    );

    this.closeUpdateForm();
  }

  closeUpdateForm(): void {

    this.showUpdateForm = false;

    this.selectedInvestmentId = null;

    this.updateForm.reset({
      currentValue: 0
    });
  }

  sellInvestment(
    id: number,
    currentValue: number
  ): void {

    this.selectedInvestmentId = id;

    this.selectedInvestmentValue =
      currentValue;

    this.showSellForm = true;

    this.showUpdateForm = false;

    this.sellForm.reset({

      accountId: 0,

      sellAmount: currentValue,

      sellDate:
        new Date()
          .toISOString()
          .substring(0, 10),

      description: ''

    });
  }

  submitSell(): void {

    if (
      this.sellForm.invalid ||
      this.selectedInvestmentId === null
    ) {
      this.sellForm.markAllAsTouched();
      return;
    }

    const formValue =
      this.sellForm.getRawValue();

    if (
      formValue.sellAmount! >
      this.selectedInvestmentValue
    ) {

      this.sellForm
        .get('sellAmount')
        ?.setErrors({
          exceedsValue: true
        });

      return;
    }

    this.store.dispatch(
      InvestmentActions.sellInvestment({

        id:
          this.selectedInvestmentId,

        accountId:
          formValue.accountId!,

        sellAmount:
          formValue.sellAmount!,

        sellDate:
          formValue.sellDate!,

        description:
          formValue.description ||
          undefined

      })
    );

    this.closeSellForm();
  }

  closeSellForm(): void {

    this.showSellForm = false;

    this.selectedInvestmentId = null;

    this.selectedInvestmentValue = 0;

    this.sellForm.reset({

      accountId: 0,

      sellAmount: 0,

      sellDate:
        new Date()
          .toISOString()
          .substring(0, 10),

      description: ''

    });
  }

  deleteInvestment(
    id: number
  ): void {

    const confirmed = confirm(
      'Are you sure you want to cancel this investment?'
    );

    if (!confirmed) {
      return;
    }

    this.store.dispatch(
      InvestmentActions.deleteInvestment({
        id
      })
    );
  }
}