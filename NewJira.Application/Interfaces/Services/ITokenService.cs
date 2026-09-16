using NewJira.Domain.Entities;

namespace NewJira.Application.Interfaces.Services;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken)> IssueTokensAsync(User user);
    Task<(string accessToken, string refreshToken)?> RefreshAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
    Task RevokeAllAsync(int userId);
}