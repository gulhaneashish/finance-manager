import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { AsyncPipe, CommonModule, CurrencyPipe, DecimalPipe } from '@angular/common';
import { Store } from '@ngrx/store';

import {
  createBudget,
  loadBudget,
  updateBudget
} from '../../../store/budgets/budget.actions';

import {
  selectBudget,
  selectBudgetError,
  selectBudgetLoading
} from '../../../store/budgets/budget.selectors';

import {
  selectAllCategories
} from '../../../store/categories/category.selectors';
import { Budget } from '../../../core/models/budget.model';


@Component({
  selector: 'app-budget-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AsyncPipe,
    DecimalPipe
  ],
  templateUrl: './budget-page.html',
  styleUrl: './budget-page.css'
})
export class BudgetPage {

  private fb = inject(FormBuilder);
  private store = inject(Store);
budget$ = this.store.select(selectBudget);

constructor() {
  this.budget$.subscribe(budget => {

    if (!budget) {
      return;
    }

    this.budgetForm.patchValue({
      year: budget.year,
      month: budget.month,
      expectedIncome: budget.income,
      expenseBudget: budget.expenseBudget,
      savingsTarget: budget.savingsTarget,
      investmentTarget: budget.investmentTarget
    });

  });
}
  // budget$ =
  //   this.store.select(selectBudget);

  loading$ =
    this.store.select(selectBudgetLoading);

  error$ =
    this.store.select(selectBudgetError);

  categories$ =
    this.store.select(selectAllCategories);

  budgetForm =
    this.fb.nonNullable.group({

      year: [
        new Date().getFullYear(),
        [
          Validators.required,
          Validators.min(2000),
          Validators.max(2100)
        ]
      ],

      month: [
        new Date().getMonth() + 1,
        [
          Validators.required,
          Validators.min(1),
          Validators.max(12)
        ]
      ],

      expectedIncome: [
        0,
        [
          Validators.required,
          Validators.min(0)
        ]
      ],

      expenseBudget: [
        0,
        [
          Validators.required,
          Validators.min(0)
        ]
      ],

      savingsTarget: [
        0,
        [
          Validators.required,
          Validators.min(0)
        ]
      ],

      investmentTarget: [
        0,
        [
          Validators.required,
          Validators.min(0)
        ]
      ]
    });

  categoryBudgets: {
    categoryId: number;
    amount: number;
  }[] = [];

 submit(): void {

  if (this.budgetForm.invalid) {
    this.budgetForm.markAllAsTouched();
    return;
  }

  const value = this.budgetForm.getRawValue();

  const budget = {
    year: value.year,
    month: value.month,
    expectedIncome: value.expectedIncome,
    expenseBudget: value.expenseBudget,
    savingsTarget: value.savingsTarget,
    investmentTarget: value.investmentTarget,
    categories: this.categoryBudgets
  };

  if (this.isEditing) {

    this.store.dispatch(
      updateBudget({
        year: value.year,
        month: value.month,
        budget
      })
    );

  } else {

    this.store.dispatch(
      createBudget({
        budget
      })
    );

  }
}

  load(): void {

    const value =
      this.budgetForm.getRawValue();

    this.store.dispatch(
      loadBudget({
        year: value.year,
        month: value.month
      })
    );
  }
  addCategory(categoryId: number): void {

    const exists =
      this.categoryBudgets.some(
        item => item.categoryId === categoryId
      );

    if (exists) {
      return;
    }

    this.categoryBudgets.push({
      categoryId,
      amount: 0
    });
  }

  removeCategory(categoryId: number): void {

    this.categoryBudgets =
      this.categoryBudgets.filter(
        item => item.categoryId !== categoryId
      );
  }

  updateCategoryAmount(
    categoryId: number,
    amount: number
  ): void {

    const category =
      this.categoryBudgets.find(
        item => item.categoryId === categoryId
      );

    if (category) {
      category.amount = amount;
    }
  }

  getCategoryAmount(
    categoryId: number
  ): number {

    return (
      this.categoryBudgets.find(
        item => item.categoryId === categoryId
      )?.amount ?? 0
    );
  }

  getCategoryTotal(): number {

    return this.categoryBudgets.reduce(
      (total, category) =>
        total + category.amount,
      0
    );
  }

  getCategoryRemaining(): number {

    const expenseBudget =
      this.budgetForm.controls.expenseBudget.value;

    return (
      expenseBudget -
      this.getCategoryTotal()
    );
  }

  isEditing = false;
  editBudget(budget: Budget): void {

  console.log('Budget received:', budget);
  console.log('Categories:', budget.categories);

  this.isEditing = true;

  this.budgetForm.patchValue({
    year: budget.year,
    month: budget.month,
    expectedIncome: budget.income,
    expenseBudget: budget.expenseBudget,
    savingsTarget: budget.savingsTarget,
    investmentTarget: budget.investmentTarget
  });

  this.categoryBudgets = budget.categories?.map(category => ({
    categoryId: category.categoryId,
    amount: category.amount
  })) ?? [];
}
}