using Microsoft.Extensions.Configuration;
using Moq;
using NewJira.Application.Helpers;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Application.Interfaces.Services;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Services;

namespace NewJira.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _emailService
            .Setup(service => service.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _roleRepository
            .Setup(repository => repository.GetRoleByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int roleId) => new Role { Id = roleId, RoleName = "Member" });

        _service = new AuthService(
            _userRepository.Object,
            _emailService.Object,
            new ConfigurationBuilder().Build(),
            _roleRepository.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUser_WhenPasswordHashMatches()
    {
        var user = new User
        {
            Email = "member@example.com",
            PasswordHash = PasswordHelper.Hash("Secret@123")
        };
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        var result = await _service.LoginAsync(user.Email!, "Secret@123");

        Assert.Same(user, result);
        _userRepository.Verify(repository => repository.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync("missing@example.com"))
            .ReturnsAsync((User?)null);

        var result = await _service.LoginAsync("missing@example.com", "Secret@123");

        Assert.Null(result);
        _userRepository.Verify(repository => repository.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsIncorrect()
    {
        var user = new User
        {
            Email = "member@example.com",
            PasswordHash = PasswordHelper.Hash("Secret@123")
        };
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        var result = await _service.LoginAsync(user.Email!, "WrongPassword");

        Assert.Null(result);
        _userRepository.Verify(repository => repository.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepository.Verify(repository => repository.SaveUserChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_RehashesLegacyPlaintextPassword()
    {
        var user = new User
        {
            Email = "legacy@example.com",
            Password = "LegacyPassword"
        };
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync(user.Email!))
            .ReturnsAsync(user);
        _userRepository
            .Setup(repository => repository.UpdateUserAsync(user))
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(repository => repository.SaveUserChangesAsync())
            .ReturnsAsync(true);

        var result = await _service.LoginAsync(user.Email!, "LegacyPassword");

        Assert.Same(user, result);
        Assert.Null(user.Password);
        Assert.True(PasswordHelper.Verify("LegacyPassword", user.PasswordHash!));
        _userRepository.Verify(repository => repository.UpdateUserAsync(user), Times.Once);
        _userRepository.Verify(repository => repository.SaveUserChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("+84901234567", "0901234567")]
    [InlineData("84901234567", "0901234567")]
    public async Task AuthenticateWithPhoneAsync_NormalizesNumberBeforeLookup(
        string input,
        string normalizedPhone)
    {
        var user = new User { PhoneNumber = normalizedPhone };
        _userRepository
            .Setup(repository => repository.GetUserByPhoneNumberAsync(normalizedPhone))
            .ReturnsAsync(user);

        var result = await _service.AuthenticateWithPhoneAsync(input);

        Assert.Same(user, result);
        _userRepository.Verify(
            repository => repository.GetUserByPhoneNumberAsync(normalizedPhone),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task AuthenticateWithPhoneAsync_ReturnsNullWithoutLookup_WhenPhoneIsBlank(string phone)
    {
        var result = await _service.AuthenticateWithPhoneAsync(phone);

        Assert.Null(result);
        _userRepository.Verify(
            repository => repository.GetUserByPhoneNumberAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailAlreadyExists()
    {
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync("existing@example.com"))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        var result = await _service.RegisterAsync(
            "existing@example.com",
            "Secret@123",
            "Existing User",
            "0901234567",
            "Member");

        Assert.Null(result);
        _userRepository.Verify(repository => repository.AddUserAsync(It.IsAny<User>()), Times.Never);
        _emailService.Verify(
            service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData("Admin", 1)]
    [InlineData("Manager", 2)]
    [InlineData("Member", 3)]
    [InlineData("Unknown", 3)]
    public async Task RegisterAsync_CreatesUserWithExpectedRoleAndSendsWelcomeEmail(
        string role,
        int expectedRoleId)
    {
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync("new@example.com"))
            .ReturnsAsync((User?)null);

        User? addedUser = null;
        _userRepository
            .Setup(repository => repository.AddUserAsync(It.IsAny<User>()))
            .Callback<User>(user => addedUser = user)
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(repository => repository.SaveUserChangesAsync())
            .ReturnsAsync(true);

        var result = await _service.RegisterAsync(
            "new@example.com",
            "Secret@123",
            "New User",
            "+84901234567",
            role);

        Assert.NotNull(result);
        Assert.NotNull(addedUser);
        Assert.Equal(expectedRoleId, addedUser.RoleId);
        Assert.Equal("0901234567", addedUser.PhoneNumber);
        Assert.True(PasswordHelper.Verify("Secret@123", addedUser.PasswordHash!));
        _userRepository.Verify(repository => repository.SaveUserChangesAsync(), Times.Once);
        _emailService.Verify(
            service => service.SendEmailAsync(
                "new@example.com",
                "Chào mừng đến với NewJira",
                It.Is<string>(message => message.Contains(role))),
            Times.Once);
    }
}
