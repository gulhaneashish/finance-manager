import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';

import { Auth } from '../../../core/services/auth';
import { logout } from '../../../store/auth/auth.actions';
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    MatIconModule
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  private authService = inject(Auth);
private store = inject(Store);
private router = inject(Router);

onLogout(): void {

  this.authService.logout();

  this.store.dispatch(logout());

  this.router.navigate(['/login']);
}
}