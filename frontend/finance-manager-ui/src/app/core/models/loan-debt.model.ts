import { LoanType } from './loan.model';

export interface LoanDebtItem {
  loanId: number;
  personName: string;
  type: LoanType;
  originalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  loanDate: string;
  dueDate: string | null;
  isOverdue: boolean;
  notes: string | null;
}

export interface LoanDebtSummary {
  totalBorrowed: number;
  totalLent: number;
  totalOwedByMe: number;
  totalOwedToMe: number;
  netDebt: number;
  loans: LoanDebtItem[];
}