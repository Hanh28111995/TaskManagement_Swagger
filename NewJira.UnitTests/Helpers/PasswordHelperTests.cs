using NewJira.Application.Helpers;
using Xunit;

namespace NewJira.UnitTests.Helpers;

public class PasswordHelperTests
{
    [Fact]
    public void Hash_ReturnsExpectedFormat_WithThreeParts()
    {
        var hash = PasswordHelper.Hash("Secret@123");

        var parts = hash.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.Equal("100000", parts[0]);
        Assert.True(Convert.FromBase64String(parts[1]).Length == 16);
        Assert.True(Convert.FromBase64String(parts[2]).Length == 32);
    }

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hash = PasswordHelper.Hash("Secret@123");
        Assert.True(PasswordHelper.Verify("Secret@123", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = PasswordHelper.Hash("Secret@123");
        Assert.False(PasswordHelper.Verify("WrongPass", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForMalformedHash()
    {
        Assert.False(PasswordHelper.Verify("abc", "not-a-valid-hash"));
    }

    [Fact]
    public void Hash_ProducesUniqueSalt_EachCall()
    {
        var h1 = PasswordHelper.Hash("same");
        var h2 = PasswordHelper.Hash("same");
        Assert.NotEqual(h1, h2);
    }
}