namespace FinanceManager.API.DTOs
{
    public class CategoryReportsDto
    {

        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }
    }
}
