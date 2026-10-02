import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject
} from '@angular/core';

import {
  RouterLink,
  RouterLinkActive,
  Router
} from '@angular/router';

import { MatIconModule } from '@angular/material/icon';

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


  @Input()
  isSidebarOpen = false;

  @Output()
  sidebarClosed = new EventEmitter<void>();

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }


  onNavigation(): void {

    this.sidebarClosed.emit();

  }


  onLogout(): void {

    this.authService.logout();

    this.store.dispatch(logout());

    this.router.navigate(['/login']);

  }

}