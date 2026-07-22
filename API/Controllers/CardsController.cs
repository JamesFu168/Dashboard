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
[Route("api/v1/cards")]
public sealed class CardsController(
    AppDbContext db,
    ICurrentUserService currentUser,
    CardAuthorizationService authorization,
    IHubContext<KanbanHub> hub) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CardDto>>> GetCards([FromQuery] string? viewMode)
    {
        var cards = await authorization
            .GetAccessibleCardsQuery(db, currentUser.UserId, currentUser.DepartmentId, viewMode)
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .OrderBy(card => card.Status)
            .ThenBy(card => card.SequenceOrder)
            .ToListAsync();

        return Ok(cards.Select(card => card.ToDto()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardDto>> GetCard(Guid id)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!CanView(card))
        {
            return Forbid();
        }

        return Ok(card.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<CardDto>> CreateCard(CreateCardRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var now = DateTimeProvider.TaiwanNow;
        var card = new Card
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = CardStatus.Plan,
            Scope = request.Scope,
            OwnerId = currentUser.UserId,
            DepartmentId = request.Scope == CardScope.Organization ? request.DepartmentId ?? currentUser.DepartmentId : null,
            DueDate = request.DueDate,
            SequenceOrder = request.SequenceOrder,
            DevOpsUrl = request.DevOpsUrl,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Cards.Add(card);
        await db.SaveChangesAsync();

        card = await GetCardWithDetails(card.Id) ?? card;
        await PublishAsync("CardCreated", card);

        return CreatedAtAction(nameof(GetCard), new { id = card.Id }, card.ToDto());
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CardDto>> UpdateCard(Guid id, UpdateCardRequest request)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanEditCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, card.UpdatedAt))
        {
            return Conflict("Card has changed since it was loaded.");
        }

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title cannot be empty.");
            }

            card.Title = request.Title.Trim();
        }

        card.Description = request.Description ?? card.Description;
        card.Scope = request.Scope ?? card.Scope;
        card.DepartmentId = card.Scope == CardScope.Organization
            ? request.DepartmentId ?? card.DepartmentId ?? currentUser.DepartmentId
            : null;
        card.DueDate = request.DueDate ?? card.DueDate;
        card.DevOpsUrl = request.DevOpsUrl ?? card.DevOpsUrl;
        card.UpdatedAt = DateTimeProvider.TaiwanNow;

        await db.SaveChangesAsync();
        await PublishAsync("CardUpdated", card);

        return Ok(card.ToDto());
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<CardDto>> MoveCard(Guid id, MoveCardRequest request)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanMoveCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        if (IsStale(request.UpdatedAt, card.UpdatedAt))
        {
            return Conflict("Card has changed since it was loaded.");
        }

        card.Status = request.Status;
        card.SequenceOrder = request.SequenceOrder;
        card.UpdatedAt = DateTimeProvider.TaiwanNow;

        await db.SaveChangesAsync();
        await PublishAsync("CardMoved", card);

        return Ok(card.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCard(Guid id)
    {
        var card = await GetCardWithDetails(id);
        if (card is null)
        {
            return NotFound();
        }

        if (!authorization.CanEditCard(currentUser.UserId, card))
        {
            return Forbid();
        }

        db.Cards.Remove(card);
        await db.SaveChangesAsync();
        await PublishAsync("CardDeleted", card);

        return NoContent();
    }

    private async Task<Card?> GetCardWithDetails(Guid id)
    {
        return await db.Cards
            .Include(card => card.Owner)
            .Include(card => card.Department)
            .Include(card => card.Tasks)
            .ThenInclude(task => task.Assignee)
            .FirstOrDefaultAsync(card => card.Id == id);
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

    private async Task PublishAsync(string eventName, Card card)
    {
        await hub.Clients.User(card.OwnerId.ToString()).SendAsync(eventName, card.ToDto());

        if (card.DepartmentId is int departmentId)
        {
            await hub.Clients.Group(KanbanHub.GetDepartmentGroup(departmentId)).SendAsync(eventName, card.ToDto());
        }
    }
}
