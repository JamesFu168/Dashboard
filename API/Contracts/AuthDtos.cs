namespace Dashboard.Api.Contracts;

/// <summary>
/// 登入請求 DTO。
/// </summary>
/// <param name="Email">使用者電子郵件</param>
/// <param name="Password">使用者密碼</param>
public sealed record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// 登入成功回應 DTO。
/// </summary>
/// <param name="AccessToken">JWT Access Token (15分鐘效期)</param>
/// <param name="RefreshToken">Refresh Token (7天效期)</param>
/// <param name="UserId">使用者 ID</param>
/// <param name="Name">使用者姓名</param>
/// <param name="Email">使用者電子郵件</param>
/// <param name="Role">使用者角色</param>
/// <param name="DepartmentId">所屬部門 ID</param>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int UserId,
    string Name,
    string Email,
    string Role,
    int DepartmentId);

/// <summary>
/// Refresh Token 換發與登出請求 DTO。
/// </summary>
/// <param name="RefreshToken">欲驗證或撤銷之 Refresh Token 字串</param>
public sealed record RefreshTokenRequest(
    string RefreshToken);

/// <summary>
/// Refresh Token 換發成功回應 DTO。
/// </summary>
/// <param name="AccessToken">新簽發之 JWT Access Token</param>
/// <param name="RefreshToken">新簽發之 Refresh Token</param>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);
