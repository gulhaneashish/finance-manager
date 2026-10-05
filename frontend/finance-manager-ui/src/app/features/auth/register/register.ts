import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';

import { Auth } from '../../../core/services/auth';
import { setUser } from '../../../store/auth/auth.actions';

export const passwordMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');

  if (!password || !confirmPassword) {
    return null;
  }

  if (confirmPassword.errors && !confirmPassword.errors['passwordMismatch']) {
    return null;
  }

  if (password.value !== confirmPassword.value) {
    confirmPassword.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  } else {
    if (confirmPassword.hasError('passwordMismatch')) {
      const errors = { ...confirmPassword.errors };
      delete errors['passwordMismatch'];
      confirmPassword.setErrors(Object.keys(errors).length ? errors : null);
    }
    return null;
  }
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(Auth);
  private router = inject(Router);
  private store = inject(Store);

  isLoading = false;
  errorMessage = '';
  successMessage = '';
  showPassword = false;
  showConfirmPassword = false;

  registerForm = this.fb.group(
    {
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(60)
        ]
      ],
      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(6)
        ]
      ],
      confirmPassword: [
        '',
        [
          Validators.required
        ]
      ],
      agreeTerms: [
        false,
        [
          Validators.requiredTrue
        ]
      ]
    },
    { validators: passwordMatchValidator }
  );

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const rawValue = this.registerForm.getRawValue();
    const registerPayload = {
      name: rawValue.name?.trim() || '',
      email: rawValue.email?.trim().toLowerCase() || '',
      password: rawValue.password || ''
    };

    this.authService.register(registerPayload).subscribe({
      next: () => {
        this.successMessage = 'Account created successfully! Setting up your workspace...';

        // Auto-login newly registered user for a seamless experience
        this.authService
          .login({
            email: registerPayload.email,
            password: registerPayload.password
          })
          .subscribe({
            next: loginResponse => {
              this.authService.saveTokens(
                loginResponse.token,
                loginResponse.refreshToken,
                loginResponse.role
              );

              this.store.dispatch(
                setUser({
                  user: {
                    userId: loginResponse.userId,
                    name: loginResponse.name,
                    email: loginResponse.email,
                    role: loginResponse.role
                  }
                })
              );

              this.isLoading = false;
              this.router.navigate(['/dashboard']);
            },
            error: () => {
              // If auto-login fails, redirect cleanly to login page
              this.isLoading = false;
              this.router.navigate(['/login'], {
                queryParams: { registered: 'true', email: registerPayload.email }
              });
            }
          });
      },
      error: error => {
        this.isLoading = false;
        this.errorMessage =
          error.error?.message ||
          error.error?.detail ||
          error.error?.title ||
          'Registration failed. Please check your information and try again.';
      }
    });
  }
}
