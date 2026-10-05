import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';

import { setUser } from '../../../store/auth/auth.actions';
import { Auth } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {

  private fb = inject(FormBuilder);
  private authService = inject(Auth);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private store = inject(Store);

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    if (params['registered']) {
      this.successMessage = 'Account created successfully! Please sign in with your credentials.';
    }
    if (params['email']) {
      this.loginForm.patchValue({ email: params['email'] });
    }
  }

  loginForm = this.fb.nonNullable.group({
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
    ]
  });

  onSubmit(): void {

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService
      .login(this.loginForm.getRawValue())
      .subscribe({
        next: response => {
          this.authService.saveTokens(response.token, response.refreshToken, response.role);

          this.store.dispatch(
            setUser({
              user: {
                userId: response.userId,
                name: response.name,
                email: response.email,
                role: response.role
              }
            })
          );

          this.isLoading = false;

          if (response.role?.toLowerCase() === 'admin') {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/dashboard']);
          }
        },

        error: error => {
          this.isLoading = false;

          this.errorMessage =
            error.error?.detail ??
            'Invalid email or password.';
        }
      });
  }
}