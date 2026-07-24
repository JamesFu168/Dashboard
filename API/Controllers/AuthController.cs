using BCrypt.Net;
using Dashboard.Api.Contracts;
using Dashboard.Api.Data;
using Dashboard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Controllers;

/// <summary>
/// 處理使用者身份驗證、Token 簽發與登出授權的控制器 (Authentication Controller)。
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    AppDbContext db,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    /// <summary>
    /// 使用者 Email 與密碼登入驗證。
    /// 驗證通過後會生成 15 分鐘效期的 Access Token 與 7 天效期的 Refresh Token。
    /// </summary>
    /// <param name="request">登入請求 DTO (包含 Email 與密碼)</param>
    /// <returns>登入成功回應 (包含 Token 與使用者基本資訊)</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 驗證 BCrypt 密碼雜湊
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        // 生成 Access Token 與 Refresh Token
        var (accessToken, refreshToken) = await jwtTokenService.GenerateTokensAsync(user);

        return Ok(new LoginResponse(
            accessToken,
            refreshToken,
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.DepartmentId));
    }

    /// <summary>
    /// 使用 Refresh Token 換發全新的 Access Token 與 Refresh Token。
    /// 舊有的 Refresh Token 將會被自動撤銷以防重複使用。
    /// </summary>
    /// <param name="request">Refresh Token 請求 DTO</param>
    /// <returns>新的 Access Token 與 Refresh Token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest("Refresh token is required.");
        }

        var user = await jwtTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (user == null)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        // 撤銷舊的 Refresh Token
        await jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        // 生成新的 Token 組合
        var (accessToken, refreshToken) = await jwtTokenService.GenerateTokensAsync(user);

        return Ok(new RefreshTokenResponse(accessToken, refreshToken));
    }

    /// <summary>
    /// 使用者登出，撤銷指定的 Refresh Token。
    /// </summary>
    /// <param name="request">包含欲撤銷 Refresh Token 的請求 DTO</param>
    /// <returns>無內容成功回應 (204 No Content)</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        }

        return NoContent();
    }
}
