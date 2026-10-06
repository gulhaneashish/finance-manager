namespace FinanceManager.API.Common;

public static class DateTimeExtensions
{
    public static DateTime ToUniversalUtc(this DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => dt
        };
    }
}
