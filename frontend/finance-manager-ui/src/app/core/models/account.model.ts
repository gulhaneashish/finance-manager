export interface Account {
  id: number;
  name: string;
  accountType: string;
  openingBalance: number;
  currentBalance: number;
  creditLimit?: number;
  isActive: boolean;
}