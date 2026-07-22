using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Api.Hubs;

public sealed class KanbanHub : Hub
{
    public Task JoinDepartmentGroup(int departmentId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetDepartmentGroup(departmentId));
    }

    public Task LeaveDepartmentGroup(int departmentId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetDepartmentGroup(departmentId));
    }

    public static string GetDepartmentGroup(int departmentId) => $"department:{departmentId}";
}
