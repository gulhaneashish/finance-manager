export interface LoanPaymentCreate {
  amount: number;
  accountId: number;
  paymentDate: string;
  notes?: string;
}