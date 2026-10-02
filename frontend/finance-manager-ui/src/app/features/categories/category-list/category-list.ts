import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { AsyncPipe, CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';

import {
  createCategory,
  deleteCategory
} from '../../../store/categories/category.actions';

import {
  selectAllCategories,
  selectCategoriesLoading,
  selectCategoriesError
} from '../../../store/categories/category.selectors';
import { DestroyRef, } from '@angular/core';
import { Actions, ofType } from '@ngrx/effects';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import {
  createCategorySuccess
} from '../../../store/categories/category.actions';
import { MatIconModule } from '@angular/material/icon';
@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AsyncPipe,
    MatIconModule
],
  templateUrl: './category-list.html',
  styleUrl: './category-list.css'
})
export class CategoryList {

  private fb = inject(FormBuilder);
  private store = inject(Store);
private actions$ = inject(Actions);
private destroyRef = inject(DestroyRef);

constructor() {

  this.actions$
    .pipe(
      ofType(createCategorySuccess),
      takeUntilDestroyed(this.destroyRef)
    )
    .subscribe(() => {

      this.categoryForm.reset({
        name: '',
        type: 'EXPENSE'
      });

    });
}
  categories$ =
    this.store.select(selectAllCategories);

  loading$ =
    this.store.select(selectCategoriesLoading);

  error$ =
    this.store.select(selectCategoriesError);

  categoryForm =
    this.fb.nonNullable.group({
      name: [
        '',
        [
          Validators.required,
          Validators.maxLength(50)
        ]
      ],

      type: [
        'EXPENSE',
        Validators.required
      ]
    });

  submit(): void {

    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const value =
      this.categoryForm.getRawValue();

    this.store.dispatch(
      createCategory({
        category: {
          name: value.name.trim(),
          type: value.type
        }
      })
    );
  }

  delete(id: number): void {

    const confirmed =
      window.confirm(
        'Are you sure you want to delete this category?'
      );

    if (!confirmed) {
      return;
    }

    this.store.dispatch(
      deleteCategory({ id })
    );
  }
}