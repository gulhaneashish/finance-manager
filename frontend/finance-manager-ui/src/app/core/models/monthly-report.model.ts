export interface CategoryReport {
  categoryId: number;
  categoryName: string;
  budget: number;
  spent: number;
  remaining: number;
  percentageUsed: number;
}

export interface MonthlyReport {
  year: number;
  month: number;

  totalIncome: number;
  totalExpenses: number;
  netCashFlow: number;

  expenseBudget: number;
  budgetSpent: number;
  budgetRemaining: number;

  savingsTarget: number;
  investmentTarget: number;

  savingsAmount: number;
  investmentAmount: number;

  totalBorrowed: number;
  totalLent: number;

  categories: CategoryReport[];
}