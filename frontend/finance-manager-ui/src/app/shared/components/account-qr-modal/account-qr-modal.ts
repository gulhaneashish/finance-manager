import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  OnChanges,
  SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { QrCodeService } from '../../../core/services/qr-code.service';
import { Account } from '../../../core/models/account.model';
import { Store } from '@ngrx/store';
import { selectUser } from '../../../store/auth/auth.selectors';
import { take } from 'rxjs';

@Component({
  selector: 'app-account-qr-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule],
  templateUrl: './account-qr-modal.html',
  styleUrl: './account-qr-modal.css'
})
export class AccountQrModal implements OnChanges {
  private qrCodeService = inject(QrCodeService);
  private store = inject(Store);

  @Input() isOpen = false;
  @Input() account: Account | null = null;
  @Output() closed = new EventEmitter<void>();

  qrDataUrl = '';
  customAmount: number | null = null;
  customNote = '';
  userName = 'User';
  copied = false;
  isGenerating = false;

  constructor() {
    this.store.select(selectUser).pipe(take(1)).subscribe(user => {
      if (user?.name) {
        this.userName = user.name;
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      this.customAmount = null;
      this.customNote = '';
      this.copied = false;
      this.generateQrCode();
    }
  }

  async generateQrCode(): Promise<void> {
    if (!this.account) return;

    this.isGenerating = true;
    try {
      const payload = this.qrCodeService.buildAccountPayload(
        this.account.id,
        this.account.name,
        this.account.accountType,
        this.userName,
        this.customAmount,
        this.customNote
      );

      this.qrDataUrl = await this.qrCodeService.generateQrCodeDataUrl(payload, {
        width: 320,
        margin: 2
      });
    } catch (err) {
      console.error('Failed to generate QR code', err);
    } finally {
      this.isGenerating = false;
    }
  }

  onParamChange(): void {
    this.generateQrCode();
  }

  downloadQr(): void {
    if (!this.qrDataUrl) return;

    const link = document.createElement('a');
    link.href = this.qrDataUrl;
    link.download = `QR_${this.account?.name || 'Account'}_${this.account?.id}.png`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  copyPayload(): void {
    if (!this.account) return;

    const payload = this.qrCodeService.buildAccountPayload(
      this.account.id,
      this.account.name,
      this.account.accountType,
      this.userName,
      this.customAmount,
      this.customNote
    );

    navigator.clipboard.writeText(JSON.stringify(payload, null, 2)).then(() => {
      this.copied = true;
      setTimeout(() => (this.copied = false), 2500);
    });
  }

  closeModal(): void {
    this.closed.emit();
  }
}
