using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dashboard.Api.Data;
using Dashboard.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Dashboard.Api.Services;

/// <summary>
/// JWT Token 與 Refresh Token 管理服務介面。
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// 為指定使用者生成新的 Access Token 與 Refresh Token。
    /// </summary>
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user);

    /// <summary>
    /// 驗證 Refresh Token 之有效性，若成功則回傳該使用者。
    /// </summary>
    Task<User?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// 撤銷指定之 Refresh Token (如登出或換發新 Token 時)。
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
}

/// <summary>
/// 處理 JWT 簽發、驗證與 Refresh Token 紀錄的服務實作。
/// </summary>
public sealed class JwtTokenService(
    IConfiguration configuration,
    AppDbContext db) : IJwtTokenService
{
    private readonly string _jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
    private readonly string _jwtIssuer = configuration["Jwt:Issuer"] ?? "Dashboard.Api";
    private readonly string _jwtAudience = configuration["Jwt:Audience"] ?? "Dashboard.Client";

    /// <summary>
    /// 生成 Access Token (15分鐘效期) 與 Refresh Token (7天效期並寫入資料庫)。
    /// </summary>
    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);
        return (accessToken, refreshToken);
    }

    /// <summary>
    /// 驗證傳入的 Refresh Token 是否存在、未被撤銷且未過期。
    /// </summary>
    public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (token == null || token.ExpiresAt < DateTimeProvider.TaiwanNow)
        {
            return null;
        }

        return token.User;
    }

    /// <summary>
    /// 將指定的 Refresh Token 標記為撤銷 (`IsRevoked = true`)。
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 簽發包含使用者 ID、姓名、Email、Role 與 DepartmentId 的 Access Token。
    /// </summary>
    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("DepartmentId", user.DepartmentId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTimeProvider.TaiwanNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 生成 64 位元加密隨機字串作為 Refresh Token，並儲存至 RefreshTokens 資料表。
    /// </summary>
    private async Task<string> GenerateRefreshTokenAsync(int userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTimeProvider.TaiwanNow.AddDays(7),
            CreatedAt = DateTimeProvider.TaiwanNow,
            IsRevoked = false
        };

        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        return refreshToken;
    }
}
