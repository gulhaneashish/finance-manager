using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Account> Accounts { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Budget> Budgets { get; set; }

    public DbSet<Loan> Loans { get; set; }

    public DbSet<LoanPayment> LoanPayments { get; set; }
    public DbSet<Investment> Investments { get; set; }
    public DbSet<CategoryBudget> CategoryBudgets { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Account>()
            .Property(a => a.OpeningBalance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Budget>()
       .Property(b => b.ExpectedIncome)
       .HasPrecision(18, 2);

        modelBuilder.Entity<Budget>()
            .Property(b => b.ExpenseBudget)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Budget>()
            .Property(b => b.SavingsTarget)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Budget>()
            .Property(b => b.InvestmentTarget)
            .HasPrecision(18, 2);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
      .HasOne(t => t.Account)
      .WithMany(a => a.Transactions)
      .HasForeignKey(t => t.AccountId)
      .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FromAccount)
            .WithMany(a => a.TransfersFrom)
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ToAccount)
            .WithMany(a => a.TransfersTo)
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loan>()
    .HasOne(l => l.User)
    .WithMany(u => u.Loans)
    .HasForeignKey(l => l.UserId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanPayment>()
            .HasOne(p => p.Loan)
            .WithMany(l => l.Payments)
            .HasForeignKey(p => p.LoanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanPayment>()
            .HasOne(p => p.Account)
            .WithMany()
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Loan>()
    .Property(l => l.OriginalAmount)
    .HasPrecision(18, 2);

        modelBuilder.Entity<LoanPayment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LoanPayment>()
    .HasOne(p => p.Transaction)
    .WithOne(t => t.LoanPayment)
    .HasForeignKey<LoanPayment>(
        p => p.TransactionId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CategoryBudget>()
    .HasOne(cb => cb.Budget)
    .WithMany(b => b.CategoryBudgets)
    .HasForeignKey(cb => cb.BudgetId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryBudget>()
            .HasOne(cb => cb.Category)
            .WithMany()
            .HasForeignKey(cb => cb.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Account>()
    .Property(a => a.CreditLimit)
    .HasPrecision(18, 2);

        modelBuilder.Entity<CategoryBudget>()
            .Property(cb => cb.Amount)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Purpose)
            .HasConversion<string>()
            .HasDefaultValue(TransactionPurpose.Normal);

        modelBuilder.Entity<Transaction>()
       .Property(t => t.Type)
       .HasConversion<string>();

        modelBuilder.Entity<Loan>()
            .Property(l => l.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Investment>()
    .Property(i => i.InvestedAmount)
    .HasPrecision(18, 2);

        modelBuilder.Entity<Investment>()
            .Property(i => i.CurrentValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Investment>()
    .HasOne(i => i.User)
    .WithMany()
    .HasForeignKey(i => i.UserId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Investment>()
    .HasOne(i => i.Transaction)
    .WithMany()
    .HasForeignKey(i => i.TransactionId)
    .OnDelete(DeleteBehavior.Restrict);

    }
}