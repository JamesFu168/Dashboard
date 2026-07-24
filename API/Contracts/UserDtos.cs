namespace Dashboard.Api.Contracts;

/// <summary>
/// 使用者摘要資訊 DTO (主要用於 Task 指派選單)。
/// </summary>
/// <param name="Id">使用者識別碼</param>
/// <param name="Name">使用者姓名</param>
public sealed record UserSummaryDto(int Id, string Name);
