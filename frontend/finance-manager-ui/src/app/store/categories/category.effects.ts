import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';

import * as CategoryActions from './category.actions';
import { CategoryService } from '../../core/services/category.service';

@Injectable()
export class CategoryEffects {

  private actions$ = inject(Actions);
  private categoryService = inject(CategoryService);

  loadCategories$ = createEffect(() =>
    this.actions$.pipe(

      ofType(CategoryActions.loadCategories),

      switchMap(() =>
        this.categoryService.getAll().pipe(

          map(categories =>
            CategoryActions.loadCategoriesSuccess({
              categories
            })
          ),

          catchError(error =>
            of(
              CategoryActions.loadCategoriesFailure({
                error: error.message
              })
            )
          )
        )
      )
    )
  );

  createCategory$ = createEffect(() =>
  this.actions$.pipe(

    ofType(CategoryActions.createCategory),

    switchMap(({ category }) =>
      this.categoryService.create(category).pipe(

        map(createdCategory =>
          CategoryActions.createCategorySuccess({
            category: createdCategory
          })
        ),

        catchError(error =>
          of(
            CategoryActions.createCategoryFailure({
              error:
                error.error?.message ||
                error.message ||
                'Failed to create category.'
            })
          )
        )
      )
    )
  )
);

deleteCategory$ = createEffect(() =>
  this.actions$.pipe(

    ofType(CategoryActions.deleteCategory),

    switchMap(({ id }) =>
      this.categoryService.delete(id).pipe(

        map(() =>
          CategoryActions.deleteCategorySuccess({
            id
          })
        ),

        catchError(error =>
          of(
            CategoryActions.deleteCategoryFailure({
              error:
                error.error?.message ||
                error.message ||
                'Failed to delete category.'
            })
          )
        )
      )
    )
  )
);
}