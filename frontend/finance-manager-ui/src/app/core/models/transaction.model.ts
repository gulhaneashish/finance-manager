
export enum TransactionType {
  Income = 'Income',
  Expense = 'Expense',
  Transfer = 'Transfer',
  CreditCard = 'CreditCard',
  Deposit = 'Deposit',
  Settlement = 'Settlement'
}

export enum TransactionPurpose {
  Normal = 'Normal',
  Savings = 'Savings',
  Investment = 'Investment',
  LoanBorrowed = 'LoanBorrowed',
  LoanLent = 'LoanLent',
  LoanRepayment = 'LoanRepayment',
  InitialDeposit = 'InitialDeposit',
  Deposit = 'Deposit',
  Income = 'Income',
  Expense = 'Expense',
  Transfer = 'Transfer',
  CreditCardPurchase = 'CreditCardPurchase',
  CreditCardPayment = 'CreditCardPayment',
  LoanReceived = 'LoanReceived',
  LoanPayment = 'LoanPayment',
  InvestmentSale = 'InvestmentSale'
}
export interface Transaction {
  id: number;
  accountId: number | null;
  fromAccountId: number | null;
  toAccountId: number | null;
  categoryId: number | null;
  amount: number;
  type: TransactionType;
  description: string;
  transactionDate: string;
  createdAt: string;
  purpose: TransactionPurpose;
}


