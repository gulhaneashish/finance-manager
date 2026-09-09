namespace FinanceManager.API.DTOs;

public class LoanDebtSummaryDto
{
    public decimal TotalBorrowed { get; set; }

    public decimal TotalLent { get; set; }

    public decimal TotalOwedByMe { get; set; }

    public decimal TotalOwedToMe { get; set; }

    public decimal NetDebt { get; set; }

    public List<LoanDebtItemDto> Loans { get; set; }
        = new List<LoanDebtItemDto>();
}