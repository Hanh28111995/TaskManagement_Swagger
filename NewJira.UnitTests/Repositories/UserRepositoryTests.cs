using Microsoft.EntityFrameworkCore;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using NewJira.Infrastructure.Repositories;

namespace NewJira.UnitTests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUserWithRolePermissions()
    {
        await using var context = CreateContext();
        var permission = new Permission { Id = 5, Code = "task.read" };
        var role = new Role
        {
            Id = 2,
            RoleName = "Manager",
            PermissionRoles = [new PermissionRole { Id = 8, Permission = permission }]
        };
        context.Roles.Add(role);
        context.Users.Add(new User
        {
            Id = 17,
            Email = "manager@example.com",
            RoleId = role.Id,
            Role = role
        });
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);
        var result = await repository.GetUserByEmailAsync("manager@example.com");

        Assert.NotNull(result);
        Assert.Equal("Manager", result.Role?.RoleName);
        Assert.Contains(result.Role!.PermissionRoles, item => item.Permission?.Code == "task.read");
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        var result = await repository.GetUserByEmailAsync("missing@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByPhoneNumberAsync_ReturnsMatchingUser()
    {
        await using var context = CreateContext();
        var role = new Role { Id = 1, RoleName = "Member" };
        context.Roles.Add(role);
        context.Users.Add(new User
        {
            Id = 17,
            PhoneNumber = "0901234567",
            RoleId = role.Id
        });
        await context.SaveChangesAsync();
        var repository = new UserRepository(context);

        var result = await repository.GetUserByPhoneNumberAsync("0901234567");

        Assert.NotNull(result);
        Assert.Equal(17, result.Id);
    }

    [Fact]
    public async Task AddUserAsync_PersistsNewUser()
    {
        await using var context = CreateContext();
        var role = new Role { Id = 1, RoleName = "Member" };
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context);

        await repository.AddUserAsync(new User
        {
            Email = "new@example.com",
            Name = "New User",
            RoleId = role.Id
        });

        Assert.True(await context.Users.AnyAsync(user => user.Email == "new@example.com"));
    }

    [Fact]
    public async Task DeleteUserAsync_RemovesExistingUser_AndIgnoresMissingUser()
    {
        await using var context = CreateContext();
        var role = new Role { Id = 1, RoleName = "Member" };
        context.Roles.Add(role);
        context.Users.Add(new User { Id = 17, RoleId = role.Id });
        await context.SaveChangesAsync();
        var repository = new UserRepository(context);

        await repository.DeleteUserAsync(17);
        await repository.DeleteUserAsync(999);

        Assert.Empty(await context.Users.ToListAsync());
    }

    private static JiraDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<JiraDbContext>()
            .UseInMemoryDatabase($"UserRepositoryTests-{Guid.NewGuid()}")
            .Options;
        return new JiraDbContext(options);
    }
}
