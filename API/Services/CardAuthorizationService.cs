using Dashboard.Api.Data;
using Dashboard.Api.Domain;

namespace Dashboard.Api.Services;

public sealed class CardAuthorizationService
{
    public bool CanEditCard(int currentUserId, Card card) => card.OwnerId == currentUserId;

    public bool CanMoveCard(int currentUserId, Card card) => card.OwnerId == currentUserId;

    public bool CanManageTasks(int currentUserId, Card card) => card.OwnerId == currentUserId;

    public bool CanToggleTask(int currentUserId, CardTask task, Card card)
    {
        return card.OwnerId == currentUserId || task.AssigneeId == currentUserId;
    }

    public IQueryable<Card> GetAccessibleCardsQuery(
        AppDbContext db,
        int userId,
        int userDepartmentId,
        string? viewMode)
    {
        if (string.Equals(viewMode, "organization", StringComparison.OrdinalIgnoreCase))
        {
            return db.Cards.Where(card =>
                card.Scope == CardScope.Organization
                && (card.DepartmentId == userDepartmentId || card.Tasks.Any(task => task.AssigneeId == userId)));
        }

        return db.Cards.Where(card => card.OwnerId == userId && card.Scope == CardScope.Personal);
    }
}
