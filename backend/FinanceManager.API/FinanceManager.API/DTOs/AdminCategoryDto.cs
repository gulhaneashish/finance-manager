namespace FinanceManager.API.DTOs;

public class AdminCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
}

public class AdminCreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "EXPENSE"; // EXPENSE or INCOME
    public int? TargetUserId { get; set; } // null means assign to system/current admin
}

public class AdminUpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "EXPENSE";
}
