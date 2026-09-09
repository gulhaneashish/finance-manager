export interface Investment {
  id: number;
  name: string;
  investmentType: InvestmentType;
  investedAmount: number;
  currentValue: number;
  profitLoss: number;
  profitLossPercentage: number;
  investmentDate: string;
  isActive: boolean;
}

export enum InvestmentType {
  MutualFund = 'MutualFund',
  Stock = 'Stock',
  FixedDeposit = 'FixedDeposit',
  Gold = 'Gold',
  Other = 'Other'
}

export interface InvestmentSummary {
  totalInvestedAmount: number;
  totalCurrentValue: number;
  totalProfitLoss: number;
  totalProfitLossPercentage: number;
  investmentCount: number;
}