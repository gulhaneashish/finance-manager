import { createAction, props } from '@ngrx/store';

import {
  Budget,
  BudgetCreate
} from '../../core/models/budget.model';

export const loadBudget = createAction(
  '[Budget] Load Budget',
  props<{
    year: number;
    month: number;
  }>()
);

export const loadBudgetSuccess = createAction(
  '[Budget] Load Budget Success',
  props<{
    budget: Budget;
  }>()
);

export const loadBudgetFailure = createAction(
  '[Budget] Load Budget Failure',
  props<{
    error: string;
  }>()
);

export const createBudget = createAction(
  '[Budget] Create Budget',
  props<{
    budget: BudgetCreate;
  }>()
);

export const createBudgetSuccess = createAction(
  '[Budget] Create Budget Success',
  props<{
    message: string;
  }>()
);

export const createBudgetFailure = createAction(
  '[Budget] Create Budget Failure',
  props<{
    error: string;
  }>()
);

export const updateBudget = createAction(
  '[Budget] Update Budget',
  props<{
    year: number;
    month: number;
    budget: BudgetCreate;
  }>()
);

export const updateBudgetSuccess = createAction(
  '[Budget] Update Budget Success',
  props<{
    message: string;
    year: number;
    month: number;
  }>()
);

export const updateBudgetFailure = createAction(
  '[Budget] Update Budget Failure',
  props<{
    error: string;
  }>()
);