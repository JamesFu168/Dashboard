namespace Dashboard.Api.Services;

/// <summary>
/// 提供統一的日期時間處理，固定使用 UTC+8 時區（台北時區）
/// </summary>
public static class DateTimeProvider
{
    private static readonly TimeZoneInfo TaipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

    /// <summary>
    /// 取得當前的台灣時間（UTC+8），並截斷至毫秒精度。
    /// 資料庫欄位 (UpdatedAt/CreatedAt) 皆設定為 HasPrecision(3)，若不截斷，
    /// 寫入後實際儲存值與此處回傳給前端的值會有次毫秒級的落差，
    /// 導致下一次操作帶入樂觀鎖比對 (IsStale) 時誤判為衝突並回傳 409。
    /// </summary>
    public static DateTime TaiwanNow
    {
        get
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaipeiTimeZone);
            return new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, now.Kind);
        }
    }
}
