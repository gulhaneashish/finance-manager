export interface DashboardSummary {
  year: number;
  month: number;
  totalBalance: number;
  totalIncome: number;
  totalExpenses: number;
  expenseBudget: number;
  budgetSpent: number;
  budgetRemaining: number;
  savingsTarget: number;
  investmentTarget: number;
}

export interface CategorySpending {
  categoryId: number;
  categoryName: string;
  amount: number;
}

export interface MonthlyCashFlow {
  year: number;
  month: number;
  monthName: string;
  income: number;
  expenses: number;
  savings?: number;
  netCashFlow: number;
}

export interface LoanDebt {
  id: number;
  personName: string;
  type: string;
  remainingAmount: number;
}

export interface LoanDebtSummary {
  totalBorrowed: number;
  totalLent: number;
  totalOwedByMe: number;
  totalOwedToMe: number;
  netDebt: number;
  loans: LoanDebt[];
}

export interface AccountSummary {
  accountId: number;
  name: string;
  accountType: string;
  balance: number;
  creditLimit: number | null;
  creditOutstanding: number;
  availableCredit: number;
}

export interface SavingsInvestmentSummary {
  year: number;
  month: number;

  savingsTarget: number;
  actualSavings: number;
  savingsProgress: number;

  investmentTarget: number;
  actualInvestment: number;
  investmentProgress: number;
}