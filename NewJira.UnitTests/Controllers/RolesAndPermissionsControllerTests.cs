using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.API.Controllers;
using NewJira.Application.DTOs.Role;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Controllers.Auth;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class RolesAndPermissionsControllerTests
{
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IPermissionRepository> _permissionRepository = new();

    [Fact]
    public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        _roleRepository.Setup(repository => repository.GetRoleByIdAsync(7))
            .ReturnsAsync((Role?)null);
        var controller = new RolesController(_roleRepository.Object);

        var result = await controller.GetRoleById(7);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AddRole_TrimsNameBeforePersisting()
    {
        Role? addedRole = null;
        _roleRepository.Setup(repository => repository.AddRoleAsync(It.IsAny<Role>()))
            .Callback<Role>(role => addedRole = role)
            .Returns(Task.CompletedTask);
        var controller = new RolesController(_roleRepository.Object);

        var result = await controller.AddRole(new RoleRequestDto
        {
            RoleName = "  Manager  ",
            RoleDescription = "Project manager"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(addedRole);
        Assert.Equal("Manager", addedRole.RoleName);
        Assert.Equal("Project manager", addedRole.RoleDescription);
    }

    [Fact]
    public async Task AssignPermissions_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        _roleRepository.Setup(repository => repository.GetRoleByIdAsync(7))
            .ReturnsAsync((Role?)null);
        var controller = new RolesController(_roleRepository.Object);

        var result = await controller.AssignPermissions(7, [1, 2]);

        Assert.IsType<NotFoundObjectResult>(result);
        _roleRepository.Verify(repository => repository.AssignPermissionsAsync(
            It.IsAny<int>(),
            It.IsAny<IEnumerable<int>>()), Times.Never);
    }

    [Fact]
    public async Task AssignPermissions_ReplacesPermissionsAndReturnsUpdatedRole()
    {
        var role = new Role { Id = 7, RoleName = "Manager" };
        _roleRepository.SetupSequence(repository => repository.GetRoleByIdAsync(7))
            .ReturnsAsync(role)
            .ReturnsAsync(role);
        _roleRepository.Setup(repository => repository.AssignPermissionsAsync(7, It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);
        var controller = new RolesController(_roleRepository.Object);

        var result = await controller.AssignPermissions(7, [1, 3]);

        Assert.IsType<OkObjectResult>(result);
        _roleRepository.Verify(
            repository => repository.AssignPermissionsAsync(7, It.Is<IEnumerable<int>>(ids =>
                ids.SequenceEqual(new[] { 1, 3 }))),
            Times.Once);
    }

    [Fact]
    public async Task CreatePermission_ReturnsBadRequest_WhenCodeIsBlank()
    {
        var controller = new PermissionController(_permissionRepository.Object);

        var result = await controller.Create(new PermissionDto { Code = " " });

        Assert.IsType<BadRequestObjectResult>(result);
        _permissionRepository.Verify(repository => repository.AddAsync(It.IsAny<Permission>()), Times.Never);
    }

    [Fact]
    public async Task CreatePermission_PersistsPermission_WhenCodeIsPresent()
    {
        Permission? addedPermission = null;
        _permissionRepository.Setup(repository => repository.AddAsync(It.IsAny<Permission>()))
            .Callback<Permission>(permission => addedPermission = permission)
            .Returns(Task.CompletedTask);
        var controller = new PermissionController(_permissionRepository.Object);

        var result = await controller.Create(new PermissionDto
        {
            Code = "project.manage",
            Description = "Manage projects"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(addedPermission);
        Assert.Equal("project.manage", addedPermission.Code);
        Assert.Equal("Manage projects", addedPermission.Description);
    }
}
