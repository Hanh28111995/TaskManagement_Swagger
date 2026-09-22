using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using NewJira.Application.Helpers;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Application.Interfaces.Services;
using NewJira.Domain.Entities;

namespace NewJira.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);

    public TokenService(IRefreshTokenRepository refreshRepo, IUserRepository userRepo, IConfiguration config)
    {
        _refreshRepo = refreshRepo;
        _userRepo = userRepo;
        _config = config;
    }

    private string Secret => _config["JwtSettings:Secret"]
                             ?? "SuperSecretKeyWithAtLeast32BytesLength!";

    public async Task<(string accessToken, string refreshToken)> IssueTokensAsync(User user)
    {
        var permissions = user.Role?.PermissionRoles?
        .Select(pr => pr.Permission?.Code ?? string.Empty)
        .Where(c => c.Length > 0);

        var accessToken = JwtHelper.GenerateToken(user, Secret, permissions );

        // Refresh token: chuỗi ngẫu nhiên, chỉ lưu HASH trong DB
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashToken(refreshToken);

        await _refreshRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(RefreshLifetime)
        });
        await _refreshRepo.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    public async Task<(string accessToken, string refreshToken)?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _refreshRepo.GetByHashAsync(tokenHash);
        if (stored == null) return null;
        
        if (stored.RevokedAt != null)
        {
            await _refreshRepo.RevokeAllForUserAsync(stored.UserId);
            await _refreshRepo.SaveChangesAsync();
            return null;
        }

        // Hết hạn
        if (stored.ExpiresAt < DateTime.UtcNow) return null;

        // ROTATION: revoke token cũ, cấp cặp mới, ghi ReplacedByTokenHash
        var user = await _userRepo.GetUserByIdAsync(stored.UserId);
        if (user == null) return null;

        var newTokens = await IssueTokensAsync(user);
        await _refreshRepo.RevokeAsync(stored.Id, HashToken(newTokens.refreshToken));
        await _refreshRepo.SaveChangesAsync();

        return newTokens;
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var stored = await _refreshRepo.GetByHashAsync(HashToken(refreshToken));
        if (stored != null)
        {
            await _refreshRepo.RevokeAsync(stored.Id, null);
            await _refreshRepo.SaveChangesAsync();
        }
    }

    public async Task RevokeAllAsync(int userId)
    {
        await _refreshRepo.RevokeAllForUserAsync(userId);
        await _refreshRepo.SaveChangesAsync();
    }

    private static string HashToken(string token)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}