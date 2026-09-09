export enum LoanType {
  Borrowed = 'Borrowed',
  Lent = 'Lent'
}

export interface Loan {
  id: number;
  personName: string;
  type: LoanType;
  originalAmount: number;
  accountId: number;
  paidAmount: number;
  remainingAmount: number;
  loanDate: string;
  dueDate: string | null;
  notes: string | null;
  isActive: boolean;
}

export interface LoanCreate {
  personName: string;
  type: LoanType;
  originalAmount: number;
  accountId: number;
  loanDate: string;
  dueDate: string | null;
  notes: string | null;
}

export interface LoanPaymentCreate {
  amount: number;
  accountId: number;
  paymentDate: string;
  notes: string | null;
}

export interface LoanUpdate {
  personName: string;
  type: LoanType;
  originalAmount: number;
  accountId: number;
  loanDate: string;
  dueDate: string | null;
  notes: string | null;
}