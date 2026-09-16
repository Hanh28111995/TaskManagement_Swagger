using NewJira.Domain.Entities;

namespace NewJira.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAsync(int id, string? replacedByTokenHash);
    Task RevokeAllForUserAsync(int userId);
    Task SaveChangesAsync();
}