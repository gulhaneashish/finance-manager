import { createReducer, on } from '@ngrx/store';


import {
  setUser,
  logout,
  authInitialized
} from './auth.actions';

import { AuthUser } from '../../core/models/auth.model';

export interface AuthState {
  user: AuthUser | null;
  isAuthenticated: boolean;
  initialized: boolean;
}

export const initialAuthState: AuthState = {
  user: null,
  isAuthenticated: false,
  initialized: false
};

export const authReducer = createReducer(

  initialAuthState,

  on(setUser, (state, { user }) => ({
    ...state,
    user,
    isAuthenticated: true
  })),

  on(logout, () => ({
    user: null,
    isAuthenticated: false,
    initialized: true
  })),

  on(authInitialized, state => ({
  ...state,
  initialized: true
}))
);