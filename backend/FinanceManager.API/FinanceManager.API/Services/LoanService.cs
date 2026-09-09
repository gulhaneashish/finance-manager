using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class LoanService
{
    private readonly FinanceDbContext _context;

    public LoanService(FinanceDbContext context)
    {
        _context = context;
    }


    // ============================================================
    // CREATE LOAN
    // ============================================================

    public async Task<LoanResponseDto> CreateAsync(
        LoanCreateDto dto,
        int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == dto.AccountId &&
                a.UserId == userId &&
                a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException(
                "Account not found.");
        }

        if (dto.OriginalAmount <= 0)
        {
            throw new InvalidOperationException(
                "Loan amount must be greater than zero.");
        }

        // --------------------------------------------------------
        // LENT
        //
        // You are giving your own money to someone.
        // Therefore your account must have enough money.
        // --------------------------------------------------------

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
            await _context.Database.BeginTransactionAsync();

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

            _context.Loans.Add(loan);

            await _context.SaveChangesAsync();


            // ----------------------------------------------------
            // BORROWED
            //
            // Money comes INTO your account.
            //
            // LENT
            //
            // Money goes OUT of your account.
            // ----------------------------------------------------

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

                TransactionDate = dto.LoanDate,

                Description =
                    dto.Type == LoanType.Borrowed
                        ? $"Borrowed from {dto.PersonName}"
                        : $"Lent to {dto.PersonName}",

                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(financialTransaction);

            await _context.SaveChangesAsync();

            loan.TransactionId = financialTransaction.Id;

            await _context.SaveChangesAsync();

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
        var loans = await _context.Loans
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


    // ============================================================
    // MAP LOAN
    // ============================================================

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


    // ============================================================
    // BUILD RESPONSE
    // ============================================================

    private async Task<LoanResponseDto> BuildResponseAsync(
        Loan loan)
    {
        var freshLoan = await _context.Loans
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
            await _context.Database.BeginTransactionAsync();

        try
        {
            // ----------------------------------------------------
            // GET LOAN
            // ----------------------------------------------------

            var loan = await _context.Loans
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l =>
                    l.Id == loanId &&
                    l.UserId == userId &&
                    l.IsActive);

            if (loan == null)
            {
                throw new InvalidOperationException(
                    "Loan not found.");
            }


            // ----------------------------------------------------
            // GET ACCOUNT
            // ----------------------------------------------------

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.AccountId &&
                    a.UserId == userId &&
                    a.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException(
                    "Account not found.");
            }


            // ----------------------------------------------------
            // CURRENT LOAN PAYMENT
            // ----------------------------------------------------

            var alreadyPaid = loan.Payments.Sum(
                p => p.Amount);

            var remaining =
                loan.OriginalAmount - alreadyPaid;


            // ----------------------------------------------------
            // VALIDATE LOAN REMAINING AMOUNT
            // ----------------------------------------------------

            if (remaining <= 0)
            {
                throw new InvalidOperationException(
                    "Loan is already fully paid.");
            }

            if (dto.Amount > remaining)
            {
                throw new InvalidOperationException(
                    $"Payment cannot exceed remaining loan amount. Remaining amount: ₹{remaining}");
            }


            // ----------------------------------------------------
            // GET CURRENT ACCOUNT BALANCE
            // ----------------------------------------------------

            var accountBalance =
                await GetAccountBalanceAsync(
                    dto.AccountId,
                    userId);


            // ----------------------------------------------------
            // IMPORTANT
            //
            // NEVER allow account balance to become negative.
            // ----------------------------------------------------

            if (dto.Amount > accountBalance)
            {
                throw new InvalidOperationException(
                    $"Insufficient balance. Available balance: ₹{accountBalance}");
            }


            // ----------------------------------------------------
            // DETERMINE TRANSACTION TYPE
            // ----------------------------------------------------
            //
            // BORROWED:
            //
            // You borrowed money earlier.
            // Now you are giving money back.
            //
            // Therefore:
            //
            // Expense
            //
            //
            // LENT:
            //
            // You gave money earlier.
            // Now the person gives money back.
            //
            // Therefore:
            //
            // Income
            // ----------------------------------------------------

            var transactionType =
                loan.Type == LoanType.Borrowed
                    ? TransactionType.Expense
                    : TransactionType.Income;


            // ----------------------------------------------------
            // CREATE TRANSACTION
            // ----------------------------------------------------

            var transaction = new Transaction
            {
                UserId = userId,

                AccountId = dto.AccountId,

                Amount = dto.Amount,

                Type = transactionType,

                Purpose =
                    TransactionPurpose.LoanRepayment,

                Description =
                    loan.Type == LoanType.Borrowed
                        ? $"Loan repayment to {loan.PersonName}"
                        : $"Loan received from {loan.PersonName}",

                TransactionDate = dto.PaymentDate,

                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(
                transaction);

            await _context.SaveChangesAsync();


            // ----------------------------------------------------
            // CREATE LOAN PAYMENT
            // ----------------------------------------------------

            var payment = new LoanPayment
            {
                LoanId = loan.Id,

                AccountId = dto.AccountId,

                TransactionId = transaction.Id,

                Amount = dto.Amount,

                PaymentDate = dto.PaymentDate,

                Notes = dto.Notes
            };

            _context.LoanPayments.Add(
                payment);

            await _context.SaveChangesAsync();


            // ----------------------------------------------------
            // COMMIT
            // ----------------------------------------------------

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
    // ACCOUNT BALANCE
    // ============================================================

    private async Task<decimal> GetAccountBalanceAsync(
        int accountId,
        int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == accountId &&
                a.UserId == userId &&
                a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException(
                "Account not found.");
        }


        // ========================================================
        // CREDIT CARD
        // ========================================================

        if (account.AccountType == "CREDIT_CARD")
        {
            var purchases = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.AccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPurchase)
                .SumAsync(t =>
                    (decimal?)t.Amount) ?? 0;

            var payments = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.ToAccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPayment)
                .SumAsync(t =>
                    (decimal?)t.Amount) ?? 0;

            return Math.Max(
                purchases - payments,
                0);
        }


        // ========================================================
        // NORMAL ACCOUNT
        // ========================================================

        var income = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Income &&
                t.Purpose !=
                    TransactionPurpose.Deposit)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // ========================================================
        // DEPOSITS / ADD MONEY
        // ========================================================

        var deposits = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose ==
                    TransactionPurpose.Deposit)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // ========================================================
        // EXPENSES
        // ========================================================

        var expenses = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Expense)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // ========================================================
        // TRANSFERS IN
        // ========================================================

        var transfersIn = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.ToAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // ========================================================
        // TRANSFERS OUT
        // ========================================================

        var transfersOut = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // ========================================================
        // CREDIT CARD PAYMENTS
        // ========================================================

        var creditCardPayments =
            await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.FromAccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPayment)
                .SumAsync(t =>
                    (decimal?)t.Amount) ?? 0;


        // ========================================================
        // FINAL BALANCE
        // ========================================================

        return account.OpeningBalance
            + income
            + deposits
            + transfersIn
            - expenses
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
            await _context.Database.BeginTransactionAsync();

        try
        {
            var loan = await _context.Loans
                .FirstOrDefaultAsync(l =>
                    l.Id == loanId &&
                    l.UserId == userId &&
                    l.IsActive);

            if (loan == null)
            {
                throw new InvalidOperationException(
                    "Loan not found.");
            }

            if (dto.OriginalAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Loan amount must be greater than zero.");
            }

            var paidAmount = await _context.LoanPayments
                .Where(p => p.LoanId == loanId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            if (dto.OriginalAmount < paidAmount)
            {
                throw new InvalidOperationException(
                    "Loan amount cannot be less than the amount already paid.");
            }

            var accountChanged =
                loan.AccountId != dto.AccountId;

            var amountChanged =
                loan.OriginalAmount != dto.OriginalAmount;

            var typeChanged =
                loan.Type != dto.Type;

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.AccountId &&
                    a.UserId == userId &&
                    a.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException(
                    "Account not found.");
            }

            // --------------------------------------------------------
            // VALIDATE BALANCE FOR LENT LOAN
            // --------------------------------------------------------

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

            // --------------------------------------------------------
            // ACCOUNT / AMOUNT CHANGED
            //
            // Delete old transaction
            // Create new transaction
            // --------------------------------------------------------

            if (accountChanged || amountChanged)
            {
                if (loan.TransactionId.HasValue)
                {
                    var oldTransaction =
                        await _context.Transactions
                            .FirstOrDefaultAsync(t =>
                                t.Id == loan.TransactionId.Value &&
                                t.UserId == userId);

                    if (oldTransaction != null)
                    {
                        _context.Transactions.Remove(oldTransaction);
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

                    TransactionDate = dto.LoanDate,

                    Description =
                        dto.Type == LoanType.Borrowed
                            ? $"Borrowed from {dto.PersonName}"
                            : $"Lent to {dto.PersonName}",

                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(newTransaction);

                await _context.SaveChangesAsync();

                loan.TransactionId = newTransaction.Id;
            }

            // --------------------------------------------------------
            // ONLY TYPE CHANGED
            //
            // Keep old transaction
            // Create additional transaction
            // --------------------------------------------------------

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

                    TransactionDate = dto.LoanDate,

                    Description =
                        dto.Type == LoanType.Borrowed
                            ? $"Borrowed from {dto.PersonName}"
                            : $"Lent to {dto.PersonName}",

                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(newTransaction);
            }

            // --------------------------------------------------------
            // UPDATE LOAN
            // --------------------------------------------------------

            loan.AccountId =
                dto.AccountId;

            loan.PersonName =
                dto.PersonName.Trim();

            loan.Type =
                dto.Type;

            loan.OriginalAmount =
                dto.OriginalAmount;

            loan.LoanDate =
                dto.LoanDate;

            loan.DueDate =
                dto.DueDate;

            loan.Notes =
                dto.Notes;

            await _context.SaveChangesAsync();

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