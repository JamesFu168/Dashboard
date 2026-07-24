using System.Security.Claims;

namespace Dashboard.Api.Services;

/// <summary>
/// 當前登入使用者資訊服務介面。
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// 當前登入使用者的識別碼
    /// </summary>
    int UserId { get; }

    /// <summary>
    /// 當前登入使用者所屬部門的識別碼
    /// </summary>
    int DepartmentId { get; }
}

/// <summary>
/// 從 HTTP Context JWT Claim 中解析當前登入者資訊的服務實作。
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    /// <summary>
    /// 取得當前使用者識別碼 (由 NameIdentifier 或 user_id claim 解析，預設 fallback 為 1)
    /// </summary>
    public int UserId => ReadIntClaim(ClaimTypes.NameIdentifier, "user_id") ?? 1;

    /// <summary>
    /// 取得當前使用者部門識別碼 (由 department_id 或 departmentId claim 解析，預設 fallback 為 1)
    /// </summary>
    public int DepartmentId => ReadIntClaim("department_id", "departmentId") ?? 1;

    /// <summary>
    /// 從 Claim 中解析整數值
    /// </summary>
    private int? ReadIntClaim(params string[] claimTypes)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

        foreach (var claimType in claimTypes)
        {
            var value = user.FindFirstValue(claimType);
            if (int.TryParse(value, out var result))
            {
                return result;
            }
        }

        return null;
    }
}
