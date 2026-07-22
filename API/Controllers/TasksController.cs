using Dashboard.Api.Contracts;
using Dashboard.Api.Data;
using Dashboard.Api.Domain;
using Dashboard.Api.Hubs;
using Dashboard.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Controllers;

[ApiController]
public sealed class TasksController(
    AppDbContext db,
    ICurrentUserService currentUser,
    CardAuthorizationService authorization,
    IHubContext<KanbanHub> hub) : ControllerBase
{
    [HttpPost("api/v1/cards/{cardId:guid}/tasks")]
    public async Task<ActionResult<CardTaskDto>> CreateTask(Guid cardId, CreateTaskRequest request)
    {
        var card = await GetCardWithTasks(cardId);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var now = DateTimeProvider.TaiwanNow;
        var task = new CardTask
        {
            Id = Guid.NewGuid(),
            CardId = card.Id,
            Title = request.Title.Trim(),
            AssigneeId = request.AssigneeId,
            SequenceOrder = request.SequenceOrder,
            DueDate = request.DueDate,
            DevOpsUrl = request.DevOpsUrl,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CardTasks.Add(task);
        card.UpdatedAt = now;
        await db.SaveChangesAsync();

        await PublishAsync("TaskUpdated", card.Id);
        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task.ToDto());
    }

    [HttpGet("api/v1/tasks/{taskId:guid}")]
    public async Task<ActionResult<CardTaskDto>> GetTask(Guid taskId)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        if (!CanView(task.Card!))
        {
            return Forbid();
        }

        return Ok(task.ToDto());
    }

    [HttpPatch("api/v1/tasks/{taskId:guid}")]
    public async Task<ActionResult<CardTaskDto>> UpdateTask(Guid taskId, UpdateTaskRequest request)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, task.UpdatedAt))
        {
            return Conflict("Task has changed since it was loaded.");
        }

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title cannot be empty.");
            }

            task.Title = request.Title.Trim();
        }

        task.SequenceOrder = request.SequenceOrder ?? task.SequenceOrder;
        task.DueDate = request.DueDate ?? task.DueDate;
        task.DevOpsUrl = request.DevOpsUrl ?? task.DevOpsUrl;
        task.UpdatedAt = DateTimeProvider.TaiwanNow;
        card.UpdatedAt = task.UpdatedAt;

        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return Ok(task.ToDto());
    }

    [HttpPatch("api/v1/tasks/{taskId:guid}/toggle")]
    public async Task<ActionResult<CardTaskDto>> ToggleTask(Guid taskId)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanToggleTask(currentUser.UserId, task, card))
        {
            return Forbid();
        }

        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTimeProvider.TaiwanNow;
        card.UpdatedAt = task.UpdatedAt;

        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return Ok(task.ToDto());
    }

    [HttpPut("api/v1/tasks/{taskId:guid}/assign")]
    public async Task<ActionResult<CardTaskDto>> AssignTask(Guid taskId, AssignTaskRequest request)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, task.UpdatedAt))
        {
            return Conflict("Task has changed since it was loaded.");
        }

        task.AssigneeId = request.AssigneeId;
        task.UpdatedAt = DateTimeProvider.TaiwanNow;
        card.UpdatedAt = task.UpdatedAt;

        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return Ok(task.ToDto());
    }

    [HttpDelete("api/v1/tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var task = await GetTaskWithCard(taskId);
        if (task is null)
        {
            return NotFound();
        }

        var card = task.Card!;
        if (!authorization.CanManageTasks(currentUser.UserId, card))
        {
            return Forbid();
        }

        db.CardTasks.Remove(task);
        card.UpdatedAt = DateTimeProvider.TaiwanNow;
        await db.SaveChangesAsync();
        await PublishAsync("TaskUpdated", card.Id);

        return NoContent();
    }

    private async Task<Card?> GetCardWithTasks(Guid cardId)
    {
        return await db.Cards
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .FirstOrDefaultAsync(card => card.Id == cardId);
    }

    private async Task<CardTask?> GetTaskWithCard(Guid taskId)
    {
        return await db.CardTasks
            .Include(task => task.Assignee)
            .Include(task => task.Card)
            .ThenInclude(card => card!.Tasks)
            .FirstOrDefaultAsync(task => task.Id == taskId);
    }

    private bool CanView(Card card)
    {
        return card.OwnerId == currentUser.UserId
            || (card.Scope == CardScope.Organization
                && (card.DepartmentId == currentUser.DepartmentId
                    || card.Tasks.Any(task => task.AssigneeId == currentUser.UserId)));
    }

    private static bool IsStale(DateTime? requestUpdatedAt, DateTime entityUpdatedAt)
    {
        return requestUpdatedAt is not null && requestUpdatedAt.Value != entityUpdatedAt;
    }

    private async Task PublishAsync(string eventName, Guid cardId)
    {
        var card = await db.Cards
            .Include(item => item.Owner)
            .Include(item => item.Department)
            .Include(item => item.Tasks)
            .ThenInclude(task => task.Assignee)
            .FirstAsync(item => item.Id == cardId);

        await hub.Clients.User(card.OwnerId.ToString()).SendAsync(eventName, card.ToDto());

        if (card.DepartmentId is int departmentId)
        {
            await hub.Clients.Group(KanbanHub.GetDepartmentGroup(departmentId)).SendAsync(eventName, card.ToDto());
        }
    }
}
