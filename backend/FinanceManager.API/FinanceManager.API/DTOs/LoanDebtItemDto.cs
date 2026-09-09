using FinanceManager.API.Models;

namespace FinanceManager.API.DTOs;

public class LoanDebtItemDto
{
    public int LoanId { get; set; }

    public string PersonName { get; set; }
        = string.Empty;

    public LoanType Type { get; set; }

    public decimal OriginalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsOverdue { get; set; }

    public string? Notes { get; set; }
}