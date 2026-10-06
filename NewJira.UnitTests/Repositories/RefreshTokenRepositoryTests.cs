using Microsoft.EntityFrameworkCore;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using NewJira.Infrastructure.Repositories;

namespace NewJira.UnitTests.Repositories;

public class RefreshTokenRepositoryTests
{
    [Fact]
    public async Task GetByHashAsync_ReturnsTokenMatchingHash()
    {
        await using var context = CreateContext();
        context.RefreshTokens.Add(new RefreshToken
        {
            Id = 1,
            UserId = 3,
            TokenHash = "stored-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync();
        var repository = new RefreshTokenRepository(context);

        var result = await repository.GetByHashAsync("stored-hash");

        Assert.NotNull(result);
        Assert.Equal(3, result.UserId);
    }

    [Fact]
    public async Task AddAsync_AndSaveChangesAsync_PersistToken()
    {
        await using var context = CreateContext();
        var repository = new RefreshTokenRepository(context);

        await repository.AddAsync(new RefreshToken
        {
            UserId = 3,
            TokenHash = "new-hash",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await repository.SaveChangesAsync();

        Assert.True(await context.RefreshTokens.AnyAsync(token => token.TokenHash == "new-hash"));
    }

    [Fact]
    public async Task RevokeAsync_SetsRevocationTimeAndReplacementHash()
    {
        await using var context = CreateContext();
        context.RefreshTokens.Add(new RefreshToken { Id = 4, TokenHash = "old-hash" });
        await context.SaveChangesAsync();
        var repository = new RefreshTokenRepository(context);

        await repository.RevokeAsync(4, "replacement-hash");

        var revokedToken = await context.RefreshTokens.FindAsync(4);
        Assert.NotNull(revokedToken?.RevokedAt);
        Assert.Equal("replacement-hash", revokedToken.ReplacedByTokenHash);
    }

    [Fact]
    public async Task RevokeAsync_DoesNothing_WhenTokenDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new RefreshTokenRepository(context);

        await repository.RevokeAsync(999, "replacement-hash");

        Assert.Empty(await context.RefreshTokens.ToListAsync());
    }

    [Fact]
    public async Task RevokeAllForUserAsync_RevokesOnlyActiveTokensForRequestedUser()
    {
        await using var context = CreateContext();
        var alreadyRevokedAt = DateTime.UtcNow.AddMinutes(-5);
        context.RefreshTokens.AddRange(
            new RefreshToken { Id = 1, UserId = 3, TokenHash = "user-3-active" },
            new RefreshToken { Id = 2, UserId = 3, TokenHash = "user-3-revoked", RevokedAt = alreadyRevokedAt },
            new RefreshToken { Id = 3, UserId = 4, TokenHash = "user-4-active" });
        await context.SaveChangesAsync();
        var repository = new RefreshTokenRepository(context);

        await repository.RevokeAllForUserAsync(3);

        var tokens = await context.RefreshTokens.OrderBy(token => token.Id).ToListAsync();
        Assert.NotNull(tokens[0].RevokedAt);
        Assert.Equal(alreadyRevokedAt, tokens[1].RevokedAt);
        Assert.Null(tokens[2].RevokedAt);
    }

    private static JiraDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<JiraDbContext>()
            .UseInMemoryDatabase($"RefreshTokenRepositoryTests-{Guid.NewGuid()}")
            .Options;
        return new JiraDbContext(options);
    }
}
