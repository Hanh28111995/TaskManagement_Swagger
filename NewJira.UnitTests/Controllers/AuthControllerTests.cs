using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.Interfaces.Services;
using NewJira.Controllers.Auth;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(
            _authService.Object,
            _tokenService.Object,
            Mock.Of<IWebHostEnvironment>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
    {
        _authService
            .Setup(service => service.LoginAsync("member@example.com", "wrong"))
            .ReturnsAsync((User?)null);

        var result = await _controller.Login(new UserLoginDto
        {
            Email = "member@example.com",
            Password = "wrong"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        _tokenService.Verify(service => service.IssueTokensAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ReturnsUserAndSetsRefreshTokenCookie_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Id = 3,
            Name = "Member",
            Email = "member@example.com",
            Role = new Role { RoleName = "Admin" }
        };
        _authService
            .Setup(service => service.LoginAsync(user.Email!, "Secret@123"))
            .ReturnsAsync(user);
        _tokenService
            .Setup(service => service.IssueTokensAsync(user))
            .ReturnsAsync(("access-token", "refresh-token"));

        var result = await _controller.Login(new UserLoginDto
        {
            Email = user.Email!,
            Password = "Secret@123"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Contains("refreshToken=refresh-token", _controller.Response.Headers.SetCookie.ToString());
        Assert.Contains("httponly", _controller.Response.Headers.SetCookie.ToString(), StringComparison.OrdinalIgnoreCase);
        _tokenService.Verify(service => service.IssueTokensAsync(user), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CheckPhone_ReturnsBadRequest_WhenPhoneIsBlank(string phone)
    {
        var result = await _controller.CheckPhone(phone);

        Assert.IsType<BadRequestObjectResult>(result);
        _authService.Verify(service => service.AuthenticateWithPhoneAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckPhone_ReturnsRegisteredStatus_WhenUserExists()
    {
        _authService
            .Setup(service => service.AuthenticateWithPhoneAsync("0901234567"))
            .ReturnsAsync(new User { PhoneNumber = "0901234567" });

        var result = Assert.IsType<OkObjectResult>(await _controller.CheckPhone("0901234567"));
        var response = Assert.IsAssignableFrom<NewJira.Application.DTOs.Common.ResponseResult<object>>(result.Value);

        Assert.True(response.IsSuccess);
        Assert.Equal("Số đã đăng ký", response.Message);
    }

    [Fact]
    public async Task ValidatePhoneCode_ReturnsBadRequest_WhenIdTokenIsMissing()
    {
        var result = await _controller.ValidatePhoneCode(new FirebaseLoginDto());

        Assert.IsType<BadRequestObjectResult>(result);
        _authService.Verify(service => service.VerifyFirebaseTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePhoneCode_ReturnsBadRequest_WhenFirebaseTokenHasNoPhone()
    {
        _authService
            .Setup(service => service.VerifyFirebaseTokenAsync("firebase-token"))
            .ReturnsAsync(new FirebaseTokenClaims { Uid = "firebase-uid" });

        var result = await _controller.ValidatePhoneCode(new FirebaseLoginDto
        {
            IdToken = "firebase-token"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        _authService.Verify(service => service.AuthenticateWithPhoneAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePhoneCode_ReturnsRegistrationStatus_WhenPhoneHasNoAccount()
    {
        _authService
            .Setup(service => service.VerifyFirebaseTokenAsync("firebase-token"))
            .ReturnsAsync(new FirebaseTokenClaims
            {
                Uid = "firebase-uid",
                PhoneNumber = "+84901234567"
            });
        _authService
            .Setup(service => service.AuthenticateWithPhoneAsync("+84901234567"))
            .ReturnsAsync((User?)null);

        var result = await _controller.ValidatePhoneCode(new FirebaseLoginDto
        {
            IdToken = "firebase-token"
        });

        Assert.IsType<OkObjectResult>(result);
        _tokenService.Verify(service => service.IssueTokensAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_ReturnsBadRequest_WhenCookieIsMissing()
    {
        var result = await _controller.Refresh();

        Assert.IsType<BadRequestObjectResult>(result);
        _tokenService.Verify(service => service.RefreshAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenRefreshTokenIsRejected()
    {
        _controller.HttpContext.Request.Headers.Cookie = "refreshToken=expired-token";
        _tokenService
            .Setup(service => service.RefreshAsync("expired-token"))
            .ReturnsAsync(((string accessToken, string refreshToken)?)null);

        var result = await _controller.Refresh();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Revoke_RevokesCookieTokenAndDeletesCookie()
    {
        _controller.HttpContext.Request.Headers.Cookie = "refreshToken=active-token";
        _tokenService
            .Setup(service => service.RevokeAsync("active-token"))
            .Returns(Task.CompletedTask);

        var result = await _controller.Revoke();

        Assert.IsType<OkObjectResult>(result);
        _tokenService.Verify(service => service.RevokeAsync("active-token"), Times.Once);
        Assert.Contains("refreshToken=", _controller.Response.Headers.SetCookie.ToString());
    }
}
