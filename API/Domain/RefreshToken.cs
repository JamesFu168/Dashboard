namespace Dashboard.Api.Domain;

/// <summary>
/// 代表 JWT Refresh Token 實體 (用於雙 Token 換發機制)。
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// Refresh Token 紀錄識別碼 (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所屬使用者識別碼
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 隨機生成的 Refresh Token 字串
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Token 到期時間 (台灣時間 UTC+8)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token 簽發時間 (台灣時間 UTC+8)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否已被撤銷 (登出或換發新 Token 時標記為 true)
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// 所屬使用者導覽屬性
    /// </summary>
    public User? User { get; set; }
}
