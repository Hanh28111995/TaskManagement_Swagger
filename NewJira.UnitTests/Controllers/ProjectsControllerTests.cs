using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.DTOs.Project;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Controllers.Projects;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectRepository> _projectRepository = new();
    private readonly Mock<ITaskRepository> _taskRepository = new();

    [Fact]
    public async Task CreateProject_ReturnsUnauthorized_WhenCreatorDoesNotMatchCurrentUser()
    {
        var controller = CreateController(("Id", "8"));

        var result = await controller.CreateProject(new CreateUpdateProjectDto
        {
            ProjectName = "Project",
            CreatorId = 9
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
        _projectRepository.Verify(repository => repository.AddProjectAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task GetProjectDetail_ReturnsNotFound_WhenProjectIsMissing()
    {
        _projectRepository.Setup(repository => repository.GetProjectByIdAsync(4))
            .ReturnsAsync((Project?)null);
        var controller = CreateController();

        var result = await controller.GetProjectDetail(4);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProject_ReturnsForbid_WhenUserIsNeitherOwnerNorManager()
    {
        _projectRepository.Setup(repository => repository.GetProjectByIdAsync(4))
            .ReturnsAsync(new Project { Id = 4, CreatorId = 9 });
        var controller = CreateController(("Id", "8"));

        var result = await controller.UpdateProject(4, new CreateUpdateProjectDto
        {
            ProjectName = "Changed"
        });

        Assert.IsType<ForbidResult>(result);
        _projectRepository.Verify(repository => repository.UpdateProjectAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProject_UpdatesProject_WhenCurrentUserIsOwner()
    {
        var project = new Project { Id = 4, CreatorId = 8, ProjectName = "Before" };
        _projectRepository.Setup(repository => repository.GetProjectByIdAsync(4))
            .ReturnsAsync(project);
        _projectRepository.Setup(repository => repository.UpdateProjectAsync(project))
            .Returns(Task.CompletedTask);
        var controller = CreateController(("Id", "8"));

        var result = await controller.UpdateProject(4, new CreateUpdateProjectDto
        {
            ProjectName = "After",
            Description = "Updated",
            CategoryId = 3
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("After", project.ProjectName);
        Assert.Equal("Updated", project.Description);
        Assert.Equal(3, project.CategoryId);
        _projectRepository.Verify(repository => repository.UpdateProjectAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_DeletesProject_WhenManagerPermissionIsPresent()
    {
        _projectRepository.Setup(repository => repository.GetProjectByIdAsync(4))
            .ReturnsAsync(new Project { Id = 4, CreatorId = 9 });
        _projectRepository.Setup(repository => repository.DeleteProjectAsync(4))
            .Returns(Task.CompletedTask);
        var controller = CreateController(("perm", "project.manage"));

        var result = await controller.DeleteProject(4);

        Assert.IsType<OkObjectResult>(result);
        _projectRepository.Verify(repository => repository.DeleteProjectAsync(4), Times.Once);
    }

    [Fact]
    public async Task GetAllProject_ReturnsInternalServerError_WhenRepositoryThrows()
    {
        _projectRepository.Setup(repository => repository.GetAllProjectsAsync())
            .ThrowsAsync(new InvalidOperationException("database unavailable"));
        var controller = CreateController();

        var result = Assert.IsType<ObjectResult>(await controller.GetAllProject());

        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    private ProjectsController CreateController(params (string Type, string Value)[] claims)
    {
        return new ProjectsController(_projectRepository.Object, _taskRepository.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        claims.Select(claim => new Claim(claim.Type, claim.Value)),
                        "UnitTest"))
                }
            }
        };
    }
}
