namespace API.Utils.Dates;

public sealed class DateTimeProvider
{
    public static DateTime Now => DateTime.UtcNow;
    public static DateTime Today => DateTime.UtcNow.Date;
    public static DateTime WarsawNow => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(Now, "CET");
}