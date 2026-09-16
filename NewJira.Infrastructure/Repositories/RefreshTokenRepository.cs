using Microsoft.EntityFrameworkCore;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;

namespace NewJira.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly JiraDbContext _context;

    public RefreshTokenRepository(JiraDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
        => await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task AddAsync(RefreshToken refreshToken)
        => await _context.RefreshTokens.AddAsync(refreshToken);

    public async Task RevokeAsync(int id, string? replacedByTokenHash)
    {
        var token = await _context.RefreshTokens.FindAsync(id);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByTokenHash = replacedByTokenHash;
        }
    }

    public async Task RevokeAllForUserAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens) t.RevokedAt = DateTime.UtcNow;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}