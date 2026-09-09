import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { Store } from '@ngrx/store';

import { selectUser } from '../../../store/auth/auth.selectors';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    AsyncPipe
  ],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header {

  private store = inject(Store);

  user$ = this.store.select(selectUser);

  showProfileMenu = false;

  toggleProfileMenu(): void {
    this.showProfileMenu = !this.showProfileMenu;
  }
}