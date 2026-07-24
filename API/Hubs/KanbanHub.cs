using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Api.Hubs;

/// <summary>
/// 處理看板即時異動廣播的 SignalR Hub。
/// </summary>
public sealed class KanbanHub : Hub
{
    /// <summary>
    /// 將當前連線加入指定部門的 SignalR 廣播群組 (`department:{departmentId}`)。
    /// </summary>
    /// <param name="departmentId">部門識別碼</param>
    public Task JoinDepartmentGroup(int departmentId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetDepartmentGroup(departmentId));
    }

    /// <summary>
    /// 將當前連線離開指定部門的 SignalR 廣播群組。
    /// </summary>
    /// <param name="departmentId">部門識別碼</param>
    public Task LeaveDepartmentGroup(int departmentId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetDepartmentGroup(departmentId));
    }

    /// <summary>
    /// 取得部門廣播群組的固定名稱格式。
    /// </summary>
    /// <param name="departmentId">部門識別碼</param>
    /// <returns>群組名稱字串 (例如 department:1)</returns>
    public static string GetDepartmentGroup(int departmentId) => $"department:{departmentId}";
}
