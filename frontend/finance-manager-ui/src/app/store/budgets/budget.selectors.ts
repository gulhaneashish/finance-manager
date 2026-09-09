import {
  createFeatureSelector,
  createSelector
} from '@ngrx/store';

import { BudgetState } from './budget.reducer';

export const selectBudgetState =
  createFeatureSelector<BudgetState>('budget');

export const selectBudget =
  createSelector(
    selectBudgetState,
    state => state.budget
  );

export const selectBudgetLoading =
  createSelector(
    selectBudgetState,
    state => state.loading
  );

export const selectBudgetError =
  createSelector(
    selectBudgetState,
    state => state.error
  );