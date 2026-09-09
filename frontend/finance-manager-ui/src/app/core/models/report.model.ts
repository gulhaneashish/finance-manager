export interface CategoryReport {
  categoryId: number | null;
  categoryName: string;
  amount: number;
}

export interface ReportSummary {
  totalIncome: number;
  totalExpense: number;
  netAmount: number;
  categoryExpenses: CategoryReport[];
}