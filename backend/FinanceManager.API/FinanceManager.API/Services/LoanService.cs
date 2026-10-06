using FinanceManager.API.Common;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILoanPaymentRepository _loanPaymentRepository;

    public LoanService(
        ILoanRepository loanRepository,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ILoanPaymentRepository loanPaymentRepository)
    {
        _loanRepository = loanRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _loanPaymentRepository = loanPaymentRepository;
    }

    // ============================================================
    // CREATE LOAN
    // ============================================================

    public async Task<LoanResponseDto> CreateAsync(
        LoanCreateDto dto,
        int userId)
    {
        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == dto.AccountId &&
            a.UserId == userId &&
            a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException("Account not found.");
        }

        if (dto.OriginalAmount <= 0)
        {
            throw new InvalidOperationException("Loan amount must be greater than zero.");
        }

        if (dto.Type == LoanType.Lent)
        {
            var balance = await GetAccountBalanceAsync(
                dto.AccountId,
                userId);

            if (dto.OriginalAmount > balance)
            {
                throw new InvalidOperationException(
                    $"Insufficient balance. Available balance: ₹{balance}");
            }
        }

        await using var dbTransaction =
            await _loanRepository.BeginTransactionAsync();

        try
        {
            var loan = new Loan
            {
                UserId = userId,
                AccountId = dto.AccountId,
                PersonName = dto.PersonName.Trim(),
                Type = dto.Type,
                OriginalAmount = dto.OriginalAmount,
                LoanDate = dto.LoanDate,
                DueDate = dto.DueDate,
                Notes = dto.Notes,
                IsActive = true
            };

            await _loanRepository.AddAsync(loan);
            await _loanRepository.SaveChangesAsync();

            var transactionType =
                dto.Type == LoanType.Borrowed
                    ? TransactionType.Income
                    : TransactionType.Expense;

            var purpose =
                dto.Type == LoanType.Borrowed
                    ? TransactionPurpose.LoanBorrowed
                    : TransactionPurpose.LoanLent;

            var financialTransaction = new Transaction
            {
                UserId = userId,
                AccountId = dto.AccountId,
                Amount = dto.OriginalAmount,
                Type = transactionType,
                Purpose = purpose,
                TransactionDate = dto.LoanDate.ToUniversalUtc(),
                Description =
                    dto.Type == LoanType.Borrowed
                        ? $"Borrowed from {dto.PersonName}"
                        : $"Lent to {dto.PersonName}",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(financialTransaction);
            await _transactionRepository.SaveChangesAsync();

            loan.TransactionId = financialTransaction.Id;
            await _loanRepository.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return await BuildResponseAsync(loan);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    // ============================================================
    // GET ALL LOANS
    // ============================================================

    public async Task<List<LoanResponseDto>> GetAllAsync(
        int userId)
    {
        var loans = await _loanRepository.Query()
            .Where(l =>
                l.UserId == userId &&
                l.IsActive)
            .Include(l => l.Payments)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        return loans
            .Select(MapToDto)
            .ToList();
    }

    private static LoanResponseDto MapToDto(
        Loan loan)
    {
        var paid = loan.Payments.Sum(
            p => p.Amount);

        return new LoanResponseDto
        {
            Id = loan.Id,
            PersonName = loan.PersonName,
            Type = loan.Type,
            OriginalAmount = loan.OriginalAmount,
            PaidAmount = paid,
            RemainingAmount = Math.Max(
                loan.OriginalAmount - paid,
                0),
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            Notes = loan.Notes,
            IsActive = loan.IsActive
        };
    }

    private async Task<LoanResponseDto> BuildResponseAsync(
        Loan loan)
    {
        var freshLoan = await _loanRepository.Query()
            .Include(l => l.Payments)
            .FirstAsync(l =>
                l.Id == loan.Id);

        return MapToDto(freshLoan);
    }

    // ============================================================
    // ADD LOAN PAYMENT
    // ============================================================

    public async Task<LoanResponseDto> AddPaymentAsync(
        int loanId,
        LoanPaymentCreateDto dto,
        int userId)
    {
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Payment amount must be greater than zero.");
        }

        await using var dbTransaction =
            await _loanRepository.BeginTransactionAsync();

        try
        {
            var loan = await _loanRepository.Query()
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l =>
                    l.Id == loanId &&
                    l.UserId == userId &&
                    l.IsActive);

            if (loan == null)
            {
                throw new InvalidOperationException("Loan not found.");
            }

            var account = await _accountRepository.FirstOrDefaultAsync(a =>
                a.Id == dto.AccountId &&
                a.UserId == userId &&
                a.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException("Account not found.");
            }

            var alreadyPaid = loan.Payments.Sum(p => p.Amount);
            var remaining = loan.OriginalAmount - alreadyPaid;

            if (remaining <= 0)
            {
                throw new InvalidOperationException("Loan is already fully paid.");
            }

            if (dto.Amount > remaining)
            {
                throw new InvalidOperationException(
                    $"Payment cannot exceed remaining loan amount. Remaining amount: ₹{remaining}");
            }

            var accountBalance = await GetAccountBalanceAsync(
                dto.AccountId,
                userId);

            if (dto.Amount > accountBalance)
            {
                throw new InvalidOperationException(
                    $"Insufficient balance. Available balance: ₹{accountBalance}");
            }

            var transactionType =
                loan.Type == LoanType.Borrowed
                    ? TransactionType.Expense
                    : TransactionType.Income;

            var transaction = new Transaction
            {
                UserId = userId,
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                Type = transactionType,
                Purpose = TransactionPurpose.LoanRepayment,
                Description =
                    loan.Type == LoanType.Borrowed
                        ? $"Loan repayment to {loan.PersonName}"
                        : $"Loan received from {loan.PersonName}",
                TransactionDate = dto.PaymentDate.ToUniversalUtc(),
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            var payment = new LoanPayment
            {
                LoanId = loan.Id,
                AccountId = dto.AccountId,
                TransactionId = transaction.Id,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                Notes = dto.Notes
            };

            await _loanPaymentRepository.AddAsync(payment);
            await _loanPaymentRepository.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return await BuildResponseAsync(loan);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task<decimal> GetAccountBalanceAsync(
        int accountId,
        int userId)
    {
        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == accountId &&
            a.UserId == userId &&
            a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException("Account not found.");
        }

        if (account.AccountType == "CREDIT_CARD")
        {
            var purchases = await _transactionRepository.Query()
                .Where(t =>
                    t.UserId == userId &&
                    t.AccountId == accountId &&
                    t.Purpose == TransactionPurpose.CreditCardPurchase)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var payments = await _transactionRepository.Query()
                .Where(t =>
                    t.UserId == userId &&
                    t.ToAccountId == accountId &&
                    t.Purpose == TransactionPurpose.CreditCardPayment)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            return Math.Max(purchases - payments, 0);
        }

        var income = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.OpeningBalance &&
                t.Description != "Opening balance" &&
                t.Purpose != TransactionPurpose.LoanBorrowed &&
                t.Purpose != TransactionPurpose.LoanReceived &&
                t.Purpose != TransactionPurpose.InvestmentSale)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var deposits = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.OpeningBalance &&
                t.Description != "Opening balance")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expenses = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Expense &&
                t.Purpose != TransactionPurpose.Investment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var transfersIn = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.ToAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var transfersOut = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var creditCardPayments = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Purpose == TransactionPurpose.CreditCardPayment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var investmentSales = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.InvestmentSale)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return account.OpeningBalance
            + income
            + deposits
            + investmentSales
            - expenses
            + transfersIn
            - transfersOut
            - creditCardPayments;
    }

    // ============================================================
    // UPDATE LOAN
    // ============================================================

    public async Task<LoanResponseDto> UpdateAsync(
        int loanId,
        LoanUpdateDto dto,
        int userId)
    {
        await using var dbTransaction =
            await _loanRepository.BeginTransactionAsync();

        try
        {
            var loan = await _loanRepository.FirstOrDefaultAsync(l =>
                l.Id == loanId &&
                l.UserId == userId &&
                l.IsActive);

            if (loan == null)
            {
                throw new InvalidOperationException("Loan not found.");
            }

            if (dto.OriginalAmount <= 0)
            {
                throw new InvalidOperationException("Loan amount must be greater than zero.");
            }

            var paidAmount = await _loanPaymentRepository.Query()
                .Where(p => p.LoanId == loanId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            if (dto.OriginalAmount < paidAmount)
            {
                throw new InvalidOperationException("Loan amount cannot be less than the amount already paid.");
            }

            var accountChanged = loan.AccountId != dto.AccountId;
            var amountChanged = loan.OriginalAmount != dto.OriginalAmount;
            var typeChanged = loan.Type != dto.Type;

            var account = await _accountRepository.FirstOrDefaultAsync(a =>
                a.Id == dto.AccountId &&
                a.UserId == userId &&
                a.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException("Account not found.");
            }

            if (dto.Type == LoanType.Lent &&
                (accountChanged || amountChanged || typeChanged))
            {
                var balance = await GetAccountBalanceAsync(
                    dto.AccountId,
                    userId);

                if (dto.OriginalAmount > balance)
                {
                    throw new InvalidOperationException(
                        $"Insufficient balance. Available balance: ₹{balance}");
                }
            }

            if (accountChanged || amountChanged)
            {
                if (loan.TransactionId.HasValue)
                {
                    var oldTransaction = await _transactionRepository.FirstOrDefaultAsync(t =>
                        t.Id == loan.TransactionId.Value &&
                        t.UserId == userId);

                    if (oldTransaction != null)
                    {
                        _transactionRepository.Remove(oldTransaction);
                    }
                }

                var newTransactionType =
                    dto.Type == LoanType.Borrowed
                        ? TransactionType.Income
                        : TransactionType.Expense;

                var newPurpose =
                    dto.Type == LoanType.Borrowed
                        ? TransactionPurpose.LoanBorrowed
                        : TransactionPurpose.LoanLent;

                var newTransaction = new Transaction
                {
                    UserId = userId,
                    AccountId = dto.AccountId,
                    Amount = dto.OriginalAmount,
                    Type = newTransactionType,
                    Purpose = newPurpose,
                    TransactionDate = dto.LoanDate.ToUniversalUtc(),
                    Description =
                        dto.Type == LoanType.Borrowed
                            ? $"Borrowed from {dto.PersonName}"
                            : $"Lent to {dto.PersonName}",
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepository.AddAsync(newTransaction);
                await _transactionRepository.SaveChangesAsync();

                loan.TransactionId = newTransaction.Id;
            }
            else if (typeChanged)
            {
                var newTransactionType =
                    dto.Type == LoanType.Borrowed
                        ? TransactionType.Income
                        : TransactionType.Expense;

                var newPurpose =
                    dto.Type == LoanType.Borrowed
                        ? TransactionPurpose.LoanBorrowed
                        : TransactionPurpose.LoanLent;

                var newTransaction = new Transaction
                {
                    UserId = userId,
                    AccountId = dto.AccountId,
                    Amount = dto.OriginalAmount,
                    Type = newTransactionType,
                    Purpose = newPurpose,
                    TransactionDate = dto.LoanDate.ToUniversalUtc(),
                    Description =
                        dto.Type == LoanType.Borrowed
                            ? $"Borrowed from {dto.PersonName}"
                            : $"Lent to {dto.PersonName}",
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepository.AddAsync(newTransaction);
            }

            loan.AccountId = dto.AccountId;
            loan.PersonName = dto.PersonName.Trim();
            loan.Type = dto.Type;
            loan.OriginalAmount = dto.OriginalAmount;
            loan.LoanDate = dto.LoanDate;
            loan.DueDate = dto.DueDate;
            loan.Notes = dto.Notes;

            await _loanRepository.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return await BuildResponseAsync(loan);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}