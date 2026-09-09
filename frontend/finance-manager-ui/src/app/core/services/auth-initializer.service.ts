import { Injectable, inject } from '@angular/core';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { firstValueFrom } from 'rxjs';

import { Store } from '@ngrx/store';
import { authInitialized } from '../../store/auth/auth.actions';
import { Auth } from './auth';
import { setUser } from '../../store/auth/auth.actions';

@Injectable({
  providedIn: 'root'
})
export class AuthInitializerService {

  private authService = inject(Auth);
  private store = inject(Store);
  private platformId = inject(PLATFORM_ID);

 async initialize(): Promise<void> {

  if (!isPlatformBrowser(this.platformId)) {
    return;
  }

  const token = localStorage.getItem('token');

  if (!token) {

    this.store.dispatch(
      authInitialized()
    );

    return;
  }

  try {

    const user = await firstValueFrom(
      this.authService.getCurrentUser()
    );

    this.store.dispatch(
      setUser({
        user
      })
    );

  } catch {

    localStorage.removeItem('token');

  } finally {

    this.store.dispatch(
      authInitialized()
    );

  }
}
}