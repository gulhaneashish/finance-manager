import { createAction, props } from '@ngrx/store';

import { Category } from '../../core/models/category.model';

export const loadCategories = createAction(
  '[Category] Load Categories'
);

export const loadCategoriesSuccess = createAction(
  '[Category] Load Categories Success',
  props<{ categories: Category[] }>()
);

export const loadCategoriesFailure = createAction(
  '[Category] Load Categories Failure',
  props<{ error: string }>()
);

export const createCategory = createAction(
  '[Category] Create Category',
  props<{
    category: {
      name: string;
      type: string;
    };
  }>()
);

export const createCategorySuccess = createAction(
  '[Category] Create Category Success',
  props<{ category: Category }>()
);

export const createCategoryFailure = createAction(
  '[Category] Create Category Failure',
  props<{ error: string }>()
);

export const deleteCategory = createAction(
  '[Category] Delete Category',
  props<{ id: number }>()
);

export const deleteCategorySuccess = createAction(
  '[Category] Delete Category Success',
  props<{ id: number }>()
);

export const deleteCategoryFailure = createAction(
  '[Category] Delete Category Failure',
  props<{ error: string }>()
);