using Dashboard.Api.Domain;

namespace Dashboard.Api.Contracts;

public sealed record CardDto(
    Guid Id,
    string Title,
    string? Description,
    CardStatus Status,
    CardScope Scope,
    int OwnerId,
    string? OwnerName,
    int? DepartmentId,
    string? DepartmentName,
    DateOnly? DueDate,
    int SequenceOrder,
    string? DevOpsUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<CardTaskDto> Tasks);

public sealed record CardTaskDto(
    Guid Id,
    Guid CardId,
    string Title,
    bool IsCompleted,
    int? AssigneeId,
    string? AssigneeName,
    int SequenceOrder,
    DateOnly? DueDate,
    string? DevOpsUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateCardRequest(
    string Title,
    string? Description,
    CardScope Scope,
    int? DepartmentId,
    DateOnly? DueDate,
    int SequenceOrder,
    string? DevOpsUrl);

public sealed record UpdateCardRequest(
    string? Title,
    string? Description,
    CardScope? Scope,
    int? DepartmentId,
    DateOnly? DueDate,
    string? DevOpsUrl,
    DateTimeOffset? UpdatedAt);

public sealed record MoveCardRequest(CardStatus Status, int SequenceOrder, DateTimeOffset? UpdatedAt);

public sealed record CreateTaskRequest(
    string Title,
    int? AssigneeId,
    int SequenceOrder,
    DateOnly? DueDate,
    string? DevOpsUrl);

public sealed record UpdateTaskRequest(
    string? Title,
    int? SequenceOrder,
    DateOnly? DueDate,
    string? DevOpsUrl,
    DateTimeOffset? UpdatedAt);

public sealed record AssignTaskRequest(int? AssigneeId, DateTimeOffset? UpdatedAt);
