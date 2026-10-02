import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject
} from '@angular/core';

import {
  AsyncPipe,
  isPlatformBrowser
} from '@angular/common';

import {
  PLATFORM_ID
} from '@angular/core';

import { RouterLink } from '@angular/router';

import { MatIconModule } from '@angular/material/icon';

import { MatButtonModule } from '@angular/material/button';

import { Store } from '@ngrx/store';

import { selectUser } from '../../../store/auth/auth.selectors';
import { ProfileModal } from '../profile-modal/profile-modal';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    AsyncPipe,
    RouterLink,
    ProfileModal
  ],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header {
  private store = inject(Store);
  private platformId = inject(PLATFORM_ID);

  user$ = this.store.select(selectUser);

  showProfileMenu = false;
  showProfileModal = false;
  isDarkMode = false;


  // Receive sidebar state from Layout
  @Input()
  isSidebarOpen = false;


  // Send toggle event to Layout
  @Output()
  sidebarToggle = new EventEmitter<void>();


  toggleProfileMenu(): void {
    this.showProfileMenu = !this.showProfileMenu;
  }

  openProfileModal(): void {
    this.showProfileMenu = false;
    this.showProfileModal = true;
  }

  closeProfileModal(): void {
    this.showProfileModal = false;
  }


  toggleSidebar(): void {

    this.sidebarToggle.emit();

  }


  toggleTheme(): void {

    this.isDarkMode =
      !this.isDarkMode;

    if (isPlatformBrowser(this.platformId)) {

      document.body.classList.toggle(
        'dark-theme',
        this.isDarkMode
      );

      localStorage.setItem(
        'theme',
        this.isDarkMode
          ? 'dark'
          : 'light'
      );

    }

  }

}