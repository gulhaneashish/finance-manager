namespace FinanceManager.API.Models;

public enum TransactionPurpose
{
    Normal = 0,

    Savings = 1,

    Investment = 2,

    LoanBorrowed = 3,

    LoanLent = 4,

    LoanRepayment = 5,

    InitialDeposit = 6,

    Deposit = 7,

    Income = 8,

    Expense = 9,

    Transfer = 10,

    CreditCardPurchase = 11,

    CreditCardPayment = 12,

    LoanReceived = 13,

    LoanPayment = 14,

    InvestmentSale = 15
}