using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.DTOs.Task;
using NewJira.Controllers.Tasks;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _repository = new();

    [Fact]
    public async Task GetAllTasks_FiltersToAssignedTasks_ForMember()
    {
        _repository.Setup(repository => repository.GetAllTasksAsync())
            .ReturnsAsync(new[]
            {
                new TaskItem { Id = 1, TaskName = "Assigned", AssigneeId = 8 },
                new TaskItem { Id = 2, TaskName = "Other", AssigneeId = 9 }
            });
        var controller = CreateController(("role", "Member"), ("Id", "8"));

        var result = Assert.IsType<OkObjectResult>(await controller.GetAllTasks());
        var response = Assert.IsAssignableFrom<NewJira.Application.DTOs.Common.ResponseResult<object>>(result.Value);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(response.Content);

        Assert.Equal(new[] { "Assigned" }, tasks.Select(task => task.TaskName));
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        _repository.Setup(repository => repository.GetTaskDetailByProjectIdAsync(4, 13))
            .ReturnsAsync((TaskItem?)null);
        var controller = CreateController(("role", "Member"), ("Id", "8"));

        var result = await controller.GetTaskById(4, 13);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateTask_AssignsCurrentUser_WhenAssigneeIsNotProvided()
    {
        TaskItem? addedTask = null;
        _repository.Setup(repository => repository.AddTaskAsync(4, It.IsAny<TaskItem>()))
            .Callback<int, TaskItem>((_, task) => addedTask = task)
            .Returns(Task.CompletedTask);
        _repository.Setup(repository => repository.GetTaskDetailByProjectIdAsync(4, It.IsAny<int>()))
            .ReturnsAsync((int _, int _) => null);
        var controller = CreateController(("role", "Member"), ("Id", "8"));
        var model = new CreateTaskDto
        {
            TaskName = "New task",
            StatusId = 1,
            PriorityId = 2,
            TaskTypeId = 3
        };

        var result = await controller.CreateTask(4, model);

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(addedTask);
        Assert.Equal("New task", addedTask.TaskName);
        Assert.Equal(4, addedTask.ProjectId);
        Assert.Equal(8, addedTask.AssigneeId);
    }

    [Fact]
    public async Task UpdateTask_ReturnsForbidden_WhenCurrentUserIsNotAssignee()
    {
        _repository.Setup(repository => repository.GetTaskDetailByProjectIdAsync(4, 13))
            .ReturnsAsync(new TaskItem { Id = 13, ProjectId = 4, AssigneeId = 9 });
        var controller = CreateController(("role", "Member"), ("Id", "8"));

        var result = await controller.UpdateTask(4, 13, new CreateTaskDto { TaskName = "Changed" });

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        _repository.Verify(repository => repository.UpdateTaskAsync(It.IsAny<int>(), It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTask_UpdatesTask_WhenPermissionClaimIsPresent()
    {
        var task = new TaskItem { Id = 13, ProjectId = 4, TaskName = "Before" };
        _repository.Setup(repository => repository.GetTaskDetailByProjectIdAsync(4, 13))
            .ReturnsAsync(task);
        _repository.Setup(repository => repository.UpdateTaskAsync(4, task))
            .Returns(Task.CompletedTask);
        var controller = CreateController(("perm", "task.update.all"));

        var result = await controller.UpdateTask(4, 13, new CreateTaskDto
        {
            TaskName = "After",
            Description = "Updated description",
            StatusId = 2,
            PriorityId = 3,
            TaskTypeId = 4
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("After", task.TaskName);
        Assert.Equal("Updated description", task.Description);
        _repository.Verify(repository => repository.UpdateTaskAsync(4, task), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskStatus_ReturnsNotFound_WhenRepositoryCannotUpdate()
    {
        _repository.Setup(repository => repository.GetTaskDetailByProjectIdAsync(4, 13))
            .ReturnsAsync(new TaskItem { Id = 13, AssigneeId = 8 });
        _repository.Setup(repository => repository.UpdateTaskStatusAsync(4, 13, It.IsAny<TaskStatusDto>()))
            .ReturnsAsync(false);
        var controller = CreateController(("role", "Member"), ("Id", "8"));

        var result = await controller.UpdateTaskStatus(4, 13, new TaskStatusDto { StatusId = 2 });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTask_DeletesTask_WhenCurrentUserIsAssignee()
    {
        _repository.Setup(repository => repository.GetTaskDetailByProjectIdAsync(4, 13))
            .ReturnsAsync(new TaskItem { Id = 13, AssigneeId = 8 });
        _repository.Setup(repository => repository.DeleteTaskAsync(4, 13))
            .Returns(Task.CompletedTask);
        var controller = CreateController(("role", "Member"), ("Id", "8"));

        var result = await controller.DeleteTask(4, 13);

        Assert.IsType<OkObjectResult>(result);
        _repository.Verify(repository => repository.DeleteTaskAsync(4, 13), Times.Once);
    }

    private TasksController CreateController(params (string Type, string Value)[] claims)
    {
        var controller = new TasksController(_repository.Object)
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
        return controller;
    }
}
