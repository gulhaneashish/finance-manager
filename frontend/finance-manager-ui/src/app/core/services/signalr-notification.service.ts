import { Injectable, inject, signal, computed, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { Auth } from './auth';

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'danger';
  timestamp: string | Date;
  read: boolean;
  data?: any;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRNotificationService {
  private auth = inject(Auth);
  private platformId = inject(PLATFORM_ID);

  private hubConnection: signalR.HubConnection | null = null;
  private isConnecting = false;

  readonly notifications = signal<AppNotification[]>([]);
  readonly activeToast = signal<AppNotification | null>(null);
  private toastTimeout: any = null;

  readonly unreadCount = computed(() =>
    this.notifications().filter(n => !n.read).length
  );

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadCachedNotifications();
      this.initConnection();
    }
  }

  initConnection(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const token = this.auth.getToken();
    if (!token) {
      console.log('[SignalR] No auth token available yet.');
      return;
    }

    if (this.hubConnection && (this.hubConnection.state === signalR.HubConnectionState.Connected || this.hubConnection.state === signalR.HubConnectionState.Connecting)) {
      return;
    }

    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
      this.isConnecting = true;
      this.hubConnection.start()
        .then(() => {
          this.isConnecting = false;
          console.log('[SignalR] Connected to NotificationHub.');
        })
        .catch(err => {
          this.isConnecting = false;
          console.warn('[SignalR] Failed to reconnect existing hub:', err?.message || err);
        });
      return;
    }

    if (this.isConnecting) {
      return;
    }

    const baseUrl = environment.apiUrl.replace(/\/api$/, '');
    const hubUrl = `${baseUrl}/hubs/notifications`;

    this.isConnecting = true;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.auth.getToken() || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      console.log('[SignalR] Received notification:', notification);
      this.handleIncomingNotification(notification);
    });

    this.hubConnection.on('BalanceUpdated', (data: any) => {
      console.log('[SignalR] Balance updated:', data);
    });

    this.hubConnection.onreconnected(() => {
      console.log('[SignalR] Reconnected to NotificationHub.');
    });

    this.hubConnection.onclose((err) => {
      this.isConnecting = false;
      console.warn('[SignalR] NotificationHub closed:', err?.message || err);
    });

    this.hubConnection.start()
      .then(() => {
        this.isConnecting = false;
        console.log('[SignalR] Connected to NotificationHub successfully.');
      })
      .catch(err => {
        this.isConnecting = false;
        console.warn('[SignalR] Connection to NotificationHub failed:', err?.message || err);
      });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  private handleIncomingNotification(incoming: any): void {
    const item: AppNotification = {
      id: incoming.id || `${Date.now()}-${Math.random().toString(36).substring(2, 7)}`,
      title: incoming.title || 'Notification',
      message: incoming.message || '',
      type: (incoming.type as any) || 'info',
      timestamp: incoming.timestamp || new Date().toISOString(),
      read: false,
      data: incoming.data
    };

    // Update notifications list
    this.notifications.update(list => [item, ...list].slice(0, 50));
    this.saveCachedNotifications();

    // Trigger floating toast
    this.triggerToast(item);
  }

  triggerToast(item: AppNotification): void {
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }

    this.activeToast.set(item);

    this.toastTimeout = setTimeout(() => {
      this.activeToast.set(null);
      this.toastTimeout = null;
    }, 4500);
  }

  dismissToast(): void {
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
      this.toastTimeout = null;
    }
    this.activeToast.set(null);
  }

  markAsRead(id: string): void {
    this.notifications.update(list =>
      list.map(n => (n.id === id ? { ...n, read: true } : n))
    );
    this.saveCachedNotifications();
  }

  markAllAsRead(): void {
    this.notifications.update(list =>
      list.map(n => ({ ...n, read: true }))
    );
    this.saveCachedNotifications();
  }

  remove(id: string): void {
    this.notifications.update(list => list.filter(n => n.id !== id));
    this.saveCachedNotifications();
  }

  clearAll(): void {
    this.notifications.set([]);
    this.saveCachedNotifications();
  }

  private saveCachedNotifications(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    try {
      localStorage.setItem('fm_notifications', JSON.stringify(this.notifications()));
    } catch {
      // Ignore quota errors
    }
  }

  private loadCachedNotifications(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    try {
      const saved = localStorage.getItem('fm_notifications');
      if (saved) {
        const parsed = JSON.parse(saved);
        if (Array.isArray(parsed)) {
          this.notifications.set(parsed);
        }
      }
    } catch {
      // Ignore parsing errors
    }
  }
}
