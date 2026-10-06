using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Controllers.Auth;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _controller = new UsersController(_repository.Object);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _repository.Setup(repository => repository.GetUserByIdAsync(8))
            .ReturnsAsync((User?)null);

        var result = await _controller.GetUserById(8);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserById_ReturnsMappedUser_WhenUserExists()
    {
        _repository.Setup(repository => repository.GetUserByIdAsync(8))
            .ReturnsAsync(new User
            {
                Id = 8,
                Email = "member@example.com",
                Name = "Member",
                Role = new Role { RoleName = "Manager" }
            });

        var result = await _controller.GetUserById(8);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<NewJira.Application.DTOs.Common.ResponseResult<object>>(okResult.Value);
        var content = Assert.IsType<UserResponseDto>(response.Content);
        Assert.Equal(8, content.Id);
        Assert.Equal("Manager", content.Role);
    }

    [Fact]
    public async Task UpdateUser_PreservesExistingValues_WhenOptionalFieldsAreNull()
    {
        var user = new User
        {
            Id = 8,
            Name = "Existing",
            Email = "existing@example.com",
            PhoneNumber = "0901234567",
            RoleId = 3
        };
        _repository.Setup(repository => repository.GetUserByIdAsync(8))
            .ReturnsAsync(user);
        _repository.Setup(repository => repository.UpdateUserAsync(user))
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateUser(8, new UpdateUserDto
        {
            Name = null!,
            Email = null!,
            PhoneNumber = null,
            RoleId = null
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Existing", user.Name);
        Assert.Equal("existing@example.com", user.Email);
        Assert.Equal("0901234567", user.PhoneNumber);
        Assert.Equal(3, user.RoleId);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _repository.Setup(repository => repository.GetUserByIdAsync(8))
            .ReturnsAsync((User?)null);

        var result = await _controller.UpdateUser(8, new UpdateUserDto { Name = "Changed" });

        Assert.IsType<NotFoundObjectResult>(result);
        _repository.Verify(repository => repository.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUser_DeletesExistingUser()
    {
        _repository.Setup(repository => repository.GetUserByIdAsync(8))
            .ReturnsAsync(new User { Id = 8 });
        _repository.Setup(repository => repository.DeleteUserAsync(8))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(8);

        Assert.IsType<OkObjectResult>(result);
        _repository.Verify(repository => repository.DeleteUserAsync(8), Times.Once);
    }
}
