using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using NewJira.Application.Helpers;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Services;

namespace NewJira.UnitTests.Services;

public class TokenServiceTests
{
    private const string Secret = "unit-test-jwt-secret";

    private readonly Mock<IRefreshTokenRepository> _refreshRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly TokenService _service;

    public TokenServiceTests()
    {
        _refreshRepository
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _refreshRepository
            .Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var configuration = new Mock<IConfiguration>();
        configuration.Setup(config => config["JwtSettings:Secret"]).Returns(Secret);

        _service = new TokenService(
            _refreshRepository.Object,
            _userRepository.Object,
            configuration.Object);
    }

    [Fact]
    public async Task IssueTokensAsync_ReturnsAccessTokenAndStoresOnlyRefreshTokenHash()
    {
        var user = new User
        {
            Id = 42,
            Email = "member@example.com",
            Name = "Member",
            Role = new Role
            {
                RoleName = "Member",
                PermissionRoles =
                [
                    new PermissionRole
                    {
                        Permission = new Permission { Code = "task.read" }
                    }
                ]
            }
        };

        var result = await _service.IssueTokensAsync(user);
        var principal = JwtHelper.ValidateToken(result.accessToken, Secret);

        Assert.NotNull(principal);
        Assert.Equal("42", principal.FindFirst("Id")?.Value);
        Assert.Contains(principal.Claims, claim =>
            claim.Type == "perm" && claim.Value == "task.read");

        _refreshRepository.Verify(
            repository => repository.AddAsync(It.Is<RefreshToken>(token =>
                token.UserId == user.Id &&
                token.TokenHash == HashToken(result.refreshToken) &&
                token.TokenHash != result.refreshToken &&
                token.ExpiresAt > token.CreatedAt &&
                token.ExpiresAt <= token.CreatedAt.AddDays(7).AddSeconds(1))),
            Times.Once);
        _refreshRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNull_WhenTokenIsNotFound()
    {
        _refreshRepository
            .Setup(repository => repository.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _service.RefreshAsync("unknown-refresh-token");

        Assert.Null(result);
        _userRepository.Verify(repository => repository.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        _refreshRepository.Verify(repository => repository.RevokeAllForUserAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_RevokesAllTokens_WhenRevokedTokenIsReused()
    {
        _refreshRepository
            .Setup(repository => repository.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken
            {
                Id = 7,
                UserId = 42,
                RevokedAt = DateTime.UtcNow.AddMinutes(-1)
            });

        var result = await _service.RefreshAsync("reused-refresh-token");

        Assert.Null(result);
        _refreshRepository.Verify(repository => repository.RevokeAllForUserAsync(42), Times.Once);
        _refreshRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        _userRepository.Verify(repository => repository.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNull_WhenStoredTokenHasExpired()
    {
        _refreshRepository
            .Setup(repository => repository.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken
            {
                Id = 7,
                UserId = 42,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
            });

        var result = await _service.RefreshAsync("expired-refresh-token");

        Assert.Null(result);
        _userRepository.Verify(repository => repository.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        _refreshRepository.Verify(repository => repository.RevokeAsync(
            It.IsAny<int>(),
            It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_IssuesNewTokensAndRotatesStoredToken()
    {
        var oldToken = new RefreshToken
        {
            Id = 7,
            UserId = 42,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        _refreshRepository
            .Setup(repository => repository.GetByHashAsync(HashToken("old-refresh-token")))
            .ReturnsAsync(oldToken);
        _userRepository
            .Setup(repository => repository.GetUserByIdAsync(42))
            .ReturnsAsync(new User { Id = 42, Email = "member@example.com" });

        var result = await _service.RefreshAsync("old-refresh-token");

        Assert.NotNull(result);
        Assert.NotEqual("old-refresh-token", result.Value.refreshToken);
        _refreshRepository.Verify(
            repository => repository.RevokeAsync(
                oldToken.Id,
                HashToken(result.Value.refreshToken)),
            Times.Once);
        _refreshRepository.Verify(
            repository => repository.AddAsync(It.Is<RefreshToken>(token =>
                token.UserId == 42 &&
                token.TokenHash == HashToken(result.Value.refreshToken))),
            Times.Once);
        _refreshRepository.Verify(repository => repository.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task RevokeAsync_DoesNothing_WhenTokenIsNotFound()
    {
        _refreshRepository
            .Setup(repository => repository.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        await _service.RevokeAsync("unknown-refresh-token");

        _refreshRepository.Verify(
            repository => repository.RevokeAsync(It.IsAny<int>(), It.IsAny<string?>()),
            Times.Never);
        _refreshRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    private static string HashToken(string token)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
