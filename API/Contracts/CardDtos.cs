using Dashboard.Api.Domain;

namespace Dashboard.Api.Contracts;

/// <summary>
/// 卡片傳輸物件 DTO。
/// </summary>
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
    DateTime? DeletedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<CardTaskDto> Tasks);

/// <summary>
/// 細項任務傳輸物件 DTO。
/// </summary>
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
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// 建立卡片請求 DTO。
/// </summary>
public sealed record CreateCardRequest(
    string Title,
    string? Description,
    CardScope Scope,
    int? DepartmentId,
    DateOnly? DueDate,
    int SequenceOrder,
    string? DevOpsUrl);

/// <summary>
/// 更新卡片內容請求 DTO。
/// </summary>
public sealed record UpdateCardRequest(
    string? Title,
    string? Description,
    CardScope? Scope,
    int? DepartmentId,
    DateOnly? DueDate,
    string? DevOpsUrl,
    DateTime? UpdatedAt);

/// <summary>
/// 移動卡片狀態與排序請求 DTO。
/// </summary>
public sealed record MoveCardRequest(CardStatus Status, int SequenceOrder, DateTime? UpdatedAt);

/// <summary>
/// 建立細項任務請求 DTO。
/// </summary>
public sealed record CreateTaskRequest(
    string Title,
    int? AssigneeId,
    int SequenceOrder,
    DateOnly? DueDate,
    string? DevOpsUrl);

/// <summary>
/// 更新細項任務內容請求 DTO。
/// </summary>
public sealed record UpdateTaskRequest(
    string? Title,
    int? SequenceOrder,
    DateOnly? DueDate,
    string? DevOpsUrl,
    DateTime? UpdatedAt);

/// <summary>
/// 變更任務指派人員請求 DTO。
/// </summary>
public sealed record AssignTaskRequest(int? AssigneeId, DateTime? UpdatedAt);
