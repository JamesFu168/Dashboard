namespace Dashboard.Api.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int UserId,
    string Name,
    string Email,
    string Role,
    int DepartmentId);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);
