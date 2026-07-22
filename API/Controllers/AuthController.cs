using BCrypt.Net;
using Dashboard.Api.Contracts;
using Dashboard.Api.Data;
using Dashboard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    AppDbContext db,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    /// <summary>
    /// 使用者登入
    /// </summary>
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

        // 驗證密碼
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        // 生成 Token
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
    /// 使用 Refresh Token 取得新的 Access Token
    /// </summary>
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

        // 生成新的 Token
        var (accessToken, refreshToken) = await jwtTokenService.GenerateTokensAsync(user);

        return Ok(new RefreshTokenResponse(accessToken, refreshToken));
    }

    /// <summary>
    /// 登出（撤銷 Refresh Token）
    /// </summary>
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
