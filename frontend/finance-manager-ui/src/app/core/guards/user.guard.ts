import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { Auth } from '../services/auth';

/**
 * Ensures admins are kept in the Admin Portal and not mixed into user personal financial views.
 */
export const userGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) {
    return true;
  }

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  if (auth.isAdmin()) {
    // Admin profile is dedicated to the admin portal
    return router.createUrlTree(['/admin']);
  }

  return true;
};
