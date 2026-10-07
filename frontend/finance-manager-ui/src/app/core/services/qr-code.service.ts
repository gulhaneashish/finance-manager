import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as QRCode from 'qrcode';
import { environment } from '../../../environments/environment';

export interface QrPaymentPayload {
  scheme: string;
  action: string;
  version: string;
  accountId: number;
  accountName: string;
  accountType?: string;
  userId?: number;
  userName?: string;
  amount?: number | null;
  note?: string | null;
  createdAt?: string;
}

export interface VerifyQrResponse {
  valid: boolean;
  accountId: number;
  accountName: string;
  accountType: string;
  ownerName: string;
  suggestedAmount?: number | null;
  note?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class QrCodeService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/QrCode`;

  buildAccountPayload(
    accountId: number,
    accountName: string,
    accountType?: string,
    userName?: string,
    amount?: number | null,
    note?: string | null
  ): QrPaymentPayload {
    return {
      scheme: 'finman',
      action: 'pay_account',
      version: '1.0',
      accountId,
      accountName,
      accountType,
      userName: userName || 'User',
      amount: amount && amount > 0 ? amount : null,
      note: note ? note.trim() : null,
      createdAt: new Date().toISOString()
    };
  }

  async generateQrCodeDataUrl(data: string | object, options?: QRCode.QRCodeToDataURLOptions): Promise<string> {
    const text = typeof data === 'string' ? data : JSON.stringify(data);
    const defaultOptions: QRCode.QRCodeToDataURLOptions = {
      width: 320,
      margin: 2,
      color: {
        dark: '#0f172a',
        light: '#ffffff'
      },
      errorCorrectionLevel: 'M'
    };

    return await QRCode.toDataURL(text, { ...defaultOptions, ...options });
  }

  parseQrPayload(raw: string): QrPaymentPayload | null {
    if (!raw || typeof raw !== 'string') {
      return null;
    }

    try {
      const parsed = JSON.parse(raw);
      if (parsed && typeof parsed === 'object' && parsed.accountId) {
        return {
          scheme: parsed.scheme || 'finman',
          action: parsed.action || 'pay_account',
          version: parsed.version || '1.0',
          accountId: Number(parsed.accountId),
          accountName: parsed.accountName || `Account #${parsed.accountId}`,
          accountType: parsed.accountType,
          userId: parsed.userId,
          userName: parsed.userName,
          amount: parsed.amount != null ? Number(parsed.amount) : null,
          note: parsed.note || null,
          createdAt: parsed.createdAt
        };
      }
    } catch {
      // Not a valid JSON payload
    }

    return null;
  }

  verifyWithBackend(qrData: string): Observable<VerifyQrResponse> {
    return this.http.post<VerifyQrResponse>(`${this.apiUrl}/verify`, { qrData });
  }

  getAccountPayloadFromBackend(accountId: number, amount?: number, note?: string): Observable<QrPaymentPayload> {
    let params: any = {};
    if (amount) params.amount = amount;
    if (note) params.note = note;

    return this.http.get<QrPaymentPayload>(`${this.apiUrl}/account/${accountId}`, { params });
  }
}
