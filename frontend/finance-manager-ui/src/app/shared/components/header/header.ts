import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject
} from '@angular/core';

import {
  AsyncPipe,
  CommonModule,
  isPlatformBrowser
} from '@angular/common';

import {
  PLATFORM_ID
} from '@angular/core';

import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Store } from '@ngrx/store';

import { selectUser } from '../../../store/auth/auth.selectors';
import { ProfileModal } from '../profile-modal/profile-modal';
import { SignalRNotificationService } from '../../../core/services/signalr-notification.service';
import { QrScannerModal } from '../qr-scanner-modal/qr-scanner-modal';
import { QrPaymentPayload } from '../../../core/services/qr-code.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    AsyncPipe,
    RouterLink,
    ProfileModal,
    QrScannerModal
  ],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header implements OnInit {
  private store = inject(Store);
  private platformId = inject(PLATFORM_ID);
  private router = inject(Router);
  readonly signalRService = inject(SignalRNotificationService);

  user$ = this.store.select(selectUser);

  showProfileMenu = false;
  showProfileModal = false;
  showNotificationsMenu = false;
  showQrScannerModal = false;
  isDarkMode = false;

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.signalRService.initConnection();
    }
  }

  // Receive sidebar state from Layout
  @Input()
  isSidebarOpen = false;

  // Send toggle event to Layout
  @Output()
  sidebarToggle = new EventEmitter<void>();

  toggleNotifications(): void {
    this.showNotificationsMenu = !this.showNotificationsMenu;
    if (this.showProfileMenu) {
      this.showProfileMenu = false;
    }
  }

  openQrScanner(): void {
    this.showQrScannerModal = true;
    this.showNotificationsMenu = false;
    this.showProfileMenu = false;
  }

  closeQrScanner(): void {
    this.showQrScannerModal = false;
  }

  onQrScanned(payload: QrPaymentPayload): void {
    this.showQrScannerModal = false;
    this.router.navigate(['/transactions'], {
      queryParams: {
        toAccountId: payload.accountId,
        toAccountName: payload.accountName,
        amount: payload.amount || null,
        note: payload.note || null,
        openTransfer: 'true'
      }
    });
  }

  toggleProfileMenu(): void {
    this.showProfileMenu = !this.showProfileMenu;
    if (this.showNotificationsMenu) {
      this.showNotificationsMenu = false;
    }
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
    this.isDarkMode = !this.isDarkMode;

    if (isPlatformBrowser(this.platformId)) {
      document.body.classList.toggle(
        'dark-theme',
        this.isDarkMode
      );

      localStorage.setItem(
        'theme',
        this.isDarkMode ? 'dark' : 'light'
      );
    }
  }
}