namespace FinanceManager.API.DTOs;

public class NetWorthDto
{
    public decimal TotalAssets { get; set; }

    public decimal BankBalance { get; set; }

    public decimal CashBalance { get; set; }

    public decimal SavingsBalance { get; set; }

    public decimal InvestmentBalance { get; set; }

    public decimal CreditCardDebt { get; set; }

    public decimal LoansPayable { get; set; }

    public decimal LoansReceivable { get; set; }

    public decimal TotalLiabilities { get; set; }

    public decimal NetWorth { get; set; }
}