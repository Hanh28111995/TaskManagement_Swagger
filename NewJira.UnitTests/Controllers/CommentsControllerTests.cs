using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.DTOs.Comment;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Controllers.Comments;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class CommentsControllerTests
{
    private readonly Mock<ICommentRepository> _repository = new();

    [Fact]
    public async Task CreateComment_ReturnsUnauthorized_WhenUserClaimIsMissing()
    {
        var controller = CreateController();

        var result = await controller.CreateComment(
            new CommentRequest { ProjectId = 4, TaskId = 13 },
            new CreateCommentDto { Content = "Comment" });

        Assert.IsType<UnauthorizedObjectResult>(result);
        _repository.Verify(repository => repository.AddCommentAsync(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task CreateComment_AssignsTaskAndAuthenticatedUser()
    {
        Comment? addedComment = null;
        _repository.Setup(repository => repository.AddCommentAsync(It.IsAny<Comment>()))
            .Callback<Comment>(comment => addedComment = comment)
            .Returns(Task.CompletedTask);
        _repository.Setup(repository => repository.GetCommentByIdAndTaskAsync(4, 13, It.IsAny<int>()))
            .ReturnsAsync((Comment?)null);
        var controller = CreateController(("Id", "8"));

        var result = await controller.CreateComment(
            new CommentRequest { ProjectId = 4, TaskId = 13 },
            new CreateCommentDto { Content = "New comment", UserId = 99 });

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(addedComment);
        Assert.Equal("New comment", addedComment.ContentComment);
        Assert.Equal(13, addedComment.TaskItemId);
        Assert.Equal(8, addedComment.UserId);
    }

    [Fact]
    public async Task UpdateComment_ReturnsForbidden_WhenUserDoesNotOwnComment()
    {
        _repository.Setup(repository => repository.GetCommentByIdAndTaskAsync(4, 13, 6))
            .ReturnsAsync(new Comment { Id = 6, UserId = 9 });
        var controller = CreateController(("Id", "8"));

        var result = await controller.UpdateComment(
            new CommentRequest { ProjectId = 4, TaskId = 13 },
            6,
            new UpdateCommentDto { Content = "Changed" });

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        _repository.Verify(repository => repository.UpdateCommentAsync(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task UpdateComment_AllowsPermissionClaim_AndPersistsContent()
    {
        var comment = new Comment { Id = 6, UserId = 9, ContentComment = "Before" };
        _repository.Setup(repository => repository.GetCommentByIdAndTaskAsync(4, 13, 6))
            .ReturnsAsync(comment);
        _repository.Setup(repository => repository.UpdateCommentAsync(comment))
            .Returns(Task.CompletedTask);
        var controller = CreateController(("perm", "task.update.all"));

        var result = await controller.UpdateComment(
            new CommentRequest { ProjectId = 4, TaskId = 13 },
            6,
            new UpdateCommentDto { Content = "After" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("After", comment.ContentComment);
        _repository.Verify(repository => repository.UpdateCommentAsync(comment), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_ReturnsNotFound_WhenCommentDoesNotExist()
    {
        _repository.Setup(repository => repository.GetCommentByIdAndTaskAsync(4, 13, 6))
            .ReturnsAsync((Comment?)null);
        var controller = CreateController(("Id", "8"));

        var result = await controller.DeleteComment(
            new CommentRequest { ProjectId = 4, TaskId = 13 },
            6);

        Assert.IsType<NotFoundObjectResult>(result);
        _repository.Verify(repository => repository.DeleteCommentAsync(It.IsAny<Comment>()), Times.Never);
    }

    private CommentsController CreateController(params (string Type, string Value)[] claims)
    {
        return new CommentsController(_repository.Object)
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
