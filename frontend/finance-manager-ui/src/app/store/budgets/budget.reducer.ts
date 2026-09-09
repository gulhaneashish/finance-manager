import { createReducer, on } from '@ngrx/store';

import * as BudgetActions from './budget.actions';
import { Budget } from '../../core/models/budget.model';
import { loadBudgetSuccess } from './budget.actions';

export interface BudgetState {
  budget: Budget | null;
  loading: boolean;
  error: string | null;
}

export const initialState: BudgetState = {
  budget: null,
  loading: false,
  error: null
};

export const budgetReducer = createReducer(

  initialState,

  on(
    BudgetActions.loadBudget,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    BudgetActions.loadBudgetSuccess,
    (state, { budget }) => ({
      ...state,
      budget,
      loading: false,
      error: null
    })
  ),

  on(
    BudgetActions.loadBudgetFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

  on(
    BudgetActions.createBudget,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    BudgetActions.createBudgetSuccess,
    state => ({
      ...state,
      loading: false,
      error: null
    })
  ),

  on(
    BudgetActions.createBudgetFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),


);