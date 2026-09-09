import { createReducer, on } from '@ngrx/store';

import {
  loadCategories,
  loadCategoriesSuccess,
  loadCategoriesFailure,
  createCategory,
  createCategorySuccess,
  createCategoryFailure,
  deleteCategory,
  deleteCategorySuccess,
  deleteCategoryFailure
} from './category.actions';

import { Category } from '../../core/models/category.model';

export interface CategoryState {
  categories: Category[];
  loading: boolean;
  creating: boolean;
  error: string | null;
}

export const initialState: CategoryState = {
  categories: [],
  loading: false,
  creating: false,
  error: null
};

export const categoryReducer = createReducer(

  initialState,

  // =========================
  // LOAD CATEGORIES
  // =========================

  on(
    loadCategories,
    state => ({
      ...state,
      loading: true,
      error: null
    })
  ),

  on(
    loadCategoriesSuccess,
    (state, { categories }) => ({
      ...state,
      categories,
      loading: false,
      error: null
    })
  ),

  on(
    loadCategoriesFailure,
    (state, { error }) => ({
      ...state,
      loading: false,
      error
    })
  ),

  // =========================
  // CREATE CATEGORY
  // =========================

  on(
    createCategory,
    state => ({
      ...state,
      creating: true,
      error: null
    })
  ),

  on(
    createCategorySuccess,
    (state, { category }) => ({
      ...state,
      categories: [
        ...state.categories,
        category
      ],
      creating: false,
      error: null
    })
  ),

  on(
    createCategoryFailure,
    (state, { error }) => ({
      ...state,
      creating: false,
      error
    })
  ),

  // =========================
  // DELETE CATEGORY
  // =========================

  on(
    deleteCategory,
    state => ({
      ...state,
      error: null
    })
  ),

  on(
    deleteCategorySuccess,
    (state, { id }) => ({
      ...state,
      categories: state.categories.filter(
        category => category.id !== id
      ),
      error: null
    })
  ),

  on(
    deleteCategoryFailure,
    (state, { error }) => ({
      ...state,
      error
    })
  )

);