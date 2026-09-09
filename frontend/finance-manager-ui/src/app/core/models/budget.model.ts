export interface CategoryBudget {
  categoryId: number;
  amount: number;
}

export interface Budget {
  id: number;
  year: number;
  month: number;
  income: number;
  expenseBudget: number;
  savingsTarget: number;
  investmentTarget: number;
  spent: number;
  remaining: number;
  categories: BudgetCategory[];
}

export interface BudgetCreate {
  year: number;
  month: number;
  expectedIncome: number;
  expenseBudget: number;
  savingsTarget: number;
  investmentTarget: number;
  categories: CategoryBudget[];
}

export interface BudgetCategory {
  categoryId: number;
  categoryName: string;
  amount: number;
}