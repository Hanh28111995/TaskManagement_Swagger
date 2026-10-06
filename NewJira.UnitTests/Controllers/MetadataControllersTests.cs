using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Controllers.Metadata;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class MetadataControllersTests
{
    [Fact]
    public async Task CreateCategory_ReturnsBadRequest_WhenNameIsBlank()
    {
        var repository = new Mock<ICategoryRepository>();
        var controller = new CategoryController(repository.Object);

        var result = await controller.CreateCategory(new CategoryController.CreateCategoryDto
        {
            CategoryName = " "
        });

        Assert.IsType<BadRequestObjectResult>(result);
        repository.Verify(repository => repository.AddProjectCategoryAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreateCategory_PersistsCategory_WhenNameIsPresent()
    {
        var repository = new Mock<ICategoryRepository>();
        Category? addedCategory = null;
        repository.Setup(item => item.AddProjectCategoryAsync(It.IsAny<Category>()))
            .Callback<Category>(category => addedCategory = category)
            .Returns(Task.CompletedTask);
        var controller = new CategoryController(repository.Object);

        var result = await controller.CreateCategory(new CategoryController.CreateCategoryDto
        {
            CategoryName = "Software"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Software", addedCategory?.CategoryName);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(item => item.GetProjectCategoryByIdAsync(5))
            .ReturnsAsync((Category?)null);
        var controller = new CategoryController(repository.Object);

        var result = await controller.UpdateCategory(5, new CategoryController.UpdateCategoryDto
        {
            CategoryName = "Updated"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateStatus_PersistsNameAndAlias()
    {
        var repository = new Mock<IStatusRepository>();
        Status? addedStatus = null;
        repository.Setup(item => item.AddStatusAsync(It.IsAny<Status>()))
            .Callback<Status>(status => addedStatus = status)
            .Returns(Task.CompletedTask);
        var controller = new StatusController(repository.Object);

        var result = await controller.CreateStatus(new StatusController.CreateStatusDto
        {
            StatusName = "In Progress",
            Alias = "doing"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("In Progress", addedStatus?.StatusName);
        Assert.Equal("doing", addedStatus?.Alias);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenStatusDoesNotExist()
    {
        var repository = new Mock<IStatusRepository>();
        repository.Setup(item => item.GetStatusByIdAsync(5))
            .ReturnsAsync((Status?)null);
        var controller = new StatusController(repository.Object);

        var result = await controller.UpdateStatus(5, new StatusController.UpdateStatusDto
        {
            StatusName = "Done",
            Alias = "done"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreatePriority_ReturnsBadRequest_WhenNameIsBlank()
    {
        var repository = new Mock<IPriorityRepository>();
        var controller = new PriorityController(repository.Object);

        var result = await controller.CreatePriority(new PriorityController.PriorityDto
        {
            PriorityName = " "
        });

        Assert.IsType<BadRequestObjectResult>(result);
        repository.Verify(item => item.AddPriorityAsync(It.IsAny<Priority>()), Times.Never);
    }

    [Fact]
    public async Task CreatePriority_PersistsPriorityRank()
    {
        var repository = new Mock<IPriorityRepository>();
        Priority? addedPriority = null;
        repository.Setup(item => item.AddPriorityAsync(It.IsAny<Priority>()))
            .Callback<Priority>(priority => addedPriority = priority)
            .Returns(Task.CompletedTask);
        var controller = new PriorityController(repository.Object);

        var result = await controller.CreatePriority(new PriorityController.PriorityDto
        {
            PriorityName = "High",
            PriorityRank = 2
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("High", addedPriority?.PriorityName);
        Assert.Equal(2, addedPriority?.PriorityRank);
    }

    [Fact]
    public async Task UpdatePriority_ReturnsNotFound_WhenPriorityDoesNotExist()
    {
        var repository = new Mock<IPriorityRepository>();
        repository.Setup(item => item.GetPriorityByIdAsync(5))
            .ReturnsAsync((Priority?)null);
        var controller = new PriorityController(repository.Object);

        var result = await controller.UpdatePriority(5, new PriorityController.PriorityDto
        {
            PriorityName = "High"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateTaskType_ReturnsBadRequest_WhenNameIsBlank()
    {
        var repository = new Mock<ITaskTypeRepository>();
        var controller = new TaskTypeController(repository.Object);

        var result = await controller.CreateTaskType(new TaskTypeController.CreateTaskTypeDto
        {
            TaskTypeName = " "
        });

        Assert.IsType<BadRequestObjectResult>(result);
        repository.Verify(item => item.AddTaskTypeAsync(It.IsAny<TaskType>()), Times.Never);
    }

    [Fact]
    public async Task CreateTaskType_PersistsTaskType_WhenNameIsPresent()
    {
        var repository = new Mock<ITaskTypeRepository>();
        TaskType? addedTaskType = null;
        repository.Setup(item => item.AddTaskTypeAsync(It.IsAny<TaskType>()))
            .Callback<TaskType>(taskType => addedTaskType = taskType)
            .Returns(Task.CompletedTask);
        var controller = new TaskTypeController(repository.Object);

        var result = await controller.CreateTaskType(new TaskTypeController.CreateTaskTypeDto
        {
            TaskTypeName = "Bug"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Bug", addedTaskType?.TaskTypeName);
    }

    [Fact]
    public async Task UpdateTaskType_ReturnsNotFound_WhenTaskTypeDoesNotExist()
    {
        var repository = new Mock<ITaskTypeRepository>();
        repository.Setup(item => item.GetTaskTypeByIdAsync(5))
            .ReturnsAsync((TaskType?)null);
        var controller = new TaskTypeController(repository.Object);

        var result = await controller.UpdateTaskType(5, new TaskTypeController.UpdateTaskTypeDto
        {
            TaskTypeName = "Bug"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
