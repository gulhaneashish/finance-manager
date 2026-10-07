import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  PLATFORM_ID,
  OnChanges,
  SimpleChanges,
  OnDestroy,
  ElementRef,
  ViewChild
} from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Html5Qrcode } from 'html5-qrcode';
import { QrCodeService, QrPaymentPayload } from '../../../core/services/qr-code.service';

@Component({
  selector: 'app-qr-scanner-modal',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './qr-scanner-modal.html',
  styleUrl: './qr-scanner-modal.css'
})
export class QrScannerModal implements OnChanges, OnDestroy {
  private platformId = inject(PLATFORM_ID);
  private qrCodeService = inject(QrCodeService);

  @Input() isOpen = false;
  @Output() closed = new EventEmitter<void>();
  @Output() qrScanned = new EventEmitter<QrPaymentPayload>();

  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  scannerActive = false;
  scanMode: 'camera' | 'file' = 'camera';
  errorMessage = '';
  isVerifying = false;
  scannedPayload: QrPaymentPayload | null = null;

  private html5QrCode: Html5Qrcode | null = null;
  private readonly readerElementId = 'qr-reader-container';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen']) {
      if (this.isOpen) {
        this.resetState();
        // Give the DOM a tick to render reader container
        setTimeout(() => {
          if (this.scanMode === 'camera') {
            this.startCameraScanner();
          }
        }, 150);
      } else {
        this.stopCameraScanner();
      }
    }
  }

  ngOnDestroy(): void {
    this.stopCameraScanner();
  }

  setMode(mode: 'camera' | 'file'): void {
    this.scanMode = mode;
    this.errorMessage = '';
    if (mode === 'camera') {
      this.startCameraScanner();
    } else {
      this.stopCameraScanner();
    }
  }

  async startCameraScanner(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;

    this.errorMessage = '';
    await this.stopCameraScanner();

    try {
      this.html5QrCode = new Html5Qrcode(this.readerElementId);
      const config = {
        fps: 10,
        qrbox: { width: 250, height: 250 },
        aspectRatio: 1.0
      };

      await this.html5QrCode.start(
        { facingMode: 'environment' },
        config,
        (decodedText: string) => this.onSuccessfulScan(decodedText),
        () => {
          // Frame scan error (expected when no QR code in frame)
        }
      );

      this.scannerActive = true;
    } catch (err: any) {
      this.scannerActive = false;
      this.errorMessage = err?.message || 'Unable to access camera. Please allow camera permissions or upload an image.';
    }
  }

  async stopCameraScanner(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || !this.html5QrCode) return;

    try {
      if (this.html5QrCode.isScanning) {
        await this.html5QrCode.stop();
      }
      this.html5QrCode.clear();
    } catch {
      // Ignore cleanup error
    } finally {
      this.html5QrCode = null;
      this.scannerActive = false;
    }
  }

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.errorMessage = '';

    try {
      if (!this.html5QrCode) {
        this.html5QrCode = new Html5Qrcode(this.readerElementId);
      }
      const decodedText = await this.html5QrCode.scanFile(file, true);
      this.onSuccessfulScan(decodedText);
    } catch (err: any) {
      this.errorMessage = 'Could not detect a valid QR code in the selected image. Please try another image.';
    } finally {
      input.value = '';
    }
  }

  private onSuccessfulScan(decodedText: string): void {
    this.stopCameraScanner();
    const payload = this.qrCodeService.parseQrPayload(decodedText);

    if (!payload || !payload.accountId) {
      this.errorMessage = 'QR code detected, but it is not a valid Finance Manager payment payload.';
      return;
    }

    this.isVerifying = true;
    this.qrCodeService.verifyWithBackend(decodedText).subscribe({
      next: verified => {
        this.isVerifying = false;
        if (verified.valid) {
          this.scannedPayload = {
            ...payload,
            accountName: verified.accountName,
            accountType: verified.accountType,
            userName: verified.ownerName,
            amount: verified.suggestedAmount ?? payload.amount,
            note: verified.note ?? payload.note
          };
        } else {
          this.scannedPayload = payload;
        }
      },
      error: () => {
        this.isVerifying = false;
        // Fallback to parsed client payload
        this.scannedPayload = payload;
      }
    });
  }

  confirmScannedPayment(): void {
    if (this.scannedPayload) {
      this.qrScanned.emit(this.scannedPayload);
      this.closeModal();
    }
  }

  retryScan(): void {
    this.resetState();
    if (this.scanMode === 'camera') {
      this.startCameraScanner();
    }
  }

  closeModal(): void {
    this.stopCameraScanner();
    this.resetState();
    this.closed.emit();
  }

  private resetState(): void {
    this.errorMessage = '';
    this.scannedPayload = null;
    this.isVerifying = false;
  }
}
