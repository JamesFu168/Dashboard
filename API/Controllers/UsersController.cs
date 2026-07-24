using Dashboard.Api.Contracts;
using Dashboard.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(AppDbContext db) : ControllerBase
{
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
