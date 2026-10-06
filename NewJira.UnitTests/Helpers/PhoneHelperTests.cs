using NewJira.Application.Helpers;
using Xunit;

namespace NewJira.UnitTests.Helpers;

public class PhoneHelperTests
{
    [Theory]
    [InlineData("+84901234567", "0901234567")]   // Firebase international → local
    [InlineData("84901234567", "0901234567")]
    [InlineData("0901234567", "0901234567")]      // đã local → giữ nguyên
    [InlineData("", "")]
    public void NormalizeToLocal_Converts_AsExpected(string input, string expected)
        => Assert.Equal(expected, PhoneHelper.NormalizeToLocal(input));

    [Theory]
    [InlineData("0901234567", "+84901234567")]
    [InlineData("+84901234567", "+84901234567")]
    public void NormalizeToInternational_Converts_AsExpected(string input, string expected)
        => Assert.Equal(expected, PhoneHelper.NormalizeToInternational(input));
}