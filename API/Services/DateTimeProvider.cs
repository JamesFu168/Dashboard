namespace Dashboard.Api.Services;

/// <summary>
/// 提供統一的日期時間處理，固定使用 UTC+8 時區（台北時區）
/// </summary>
public static class DateTimeProvider
{
    private static readonly TimeZoneInfo TaipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

    /// <summary>
    /// 取得當前的台灣時間（UTC+8）
    /// </summary>
    public static DateTime TaiwanNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaipeiTimeZone);
}
