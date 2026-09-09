import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreditCardPurchaseCreate,
  CreditCardPaymentCreate,
  CreditCardPurchaseResponse,
  CreditCardPaymentResponse
} from '../models/credit-card.model';

@Injectable({
  providedIn: 'root'
})
export class CreditCardService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5228/api/CreditCard';

  purchase(
    data: CreditCardPurchaseCreate
  ): Observable<CreditCardPurchaseResponse> {

    return this.http.post<CreditCardPurchaseResponse>(
      `${this.apiUrl}/purchase`,
      data
    );
  }

  payment(
    data: CreditCardPaymentCreate
  ): Observable<CreditCardPaymentResponse> {

    return this.http.post<CreditCardPaymentResponse>(
      `${this.apiUrl}/payment`,
      data
    );
  }
}