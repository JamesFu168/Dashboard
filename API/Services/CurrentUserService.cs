using System.Security.Claims;

namespace Dashboard.Api.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    int DepartmentId { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId => ReadIntClaim(ClaimTypes.NameIdentifier, "user_id") ?? 1;
    public int DepartmentId => ReadIntClaim("department_id", "departmentId") ?? 1;

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
