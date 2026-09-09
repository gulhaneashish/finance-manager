import { createAction, props } from '@ngrx/store';
import { AuthUser } from '../../core/models/auth.model';

export const setUser = createAction(
  '[Auth] Set User',
  props<{
    user: AuthUser;
  }>()
);

export const logout = createAction(
  '[Auth] Logout'
);

export const authInitialized = createAction(
  '[Auth] Auth Initialized'
);