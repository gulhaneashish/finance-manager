export interface CreditCardPurchaseCreate {
  accountId: number;
  categoryId: number;
  amount: number;
  description?: string;
  transactionDate: string;
}

export interface CreditCardPaymentCreate {
  fromAccountId: number;
  creditCardAccountId: number;
  amount: number;
  description?: string;
  paymentDate: string;
}

export interface CreditCardPurchaseResponse {
  id: number;
  accountId: number;
  categoryId: number;
  amount: number;
  type: string;
  purpose: string;
  description: string;
  transactionDate: string;
}

export interface CreditCardPaymentResponse {
  id: number;
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  type: string;
  purpose: string;
  description: string;
  paymentDate: string;
}