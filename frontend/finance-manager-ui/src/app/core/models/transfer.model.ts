export interface TransferCreate {
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  transactionDate: string;
  description?: string;
}