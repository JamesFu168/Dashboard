using Dashboard.Api.Contracts;
using Dashboard.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Controllers;

/// <summary>
/// 提供成員選單查詢服務之控制器 (Users Controller)。
/// </summary>
[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// 取得系統使用者摘要清單 (ID 與 姓名)，提供前端 Task 指派下拉選單使用。
    /// 可傳入 departmentId 進行特定部門成員過濾。
    /// </summary>
    /// <param name="departmentId">可選的部門識別碼過濾條件</param>
    /// <returns>使用者摘要 DTO 集合</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserSummaryDto>>> GetUsers([FromQuery] int? departmentId)
    {
        var query = db.Users.AsQueryable();
        if (departmentId is int dept)
        {
            query = query.Where(u => u.DepartmentId == dept);
        }

        var users = await query
            .OrderBy(u => u.Name)
            .Select(u => new UserSummaryDto(u.Id, u.Name))
            .ToListAsync();

        return Ok(users);
    }
}
