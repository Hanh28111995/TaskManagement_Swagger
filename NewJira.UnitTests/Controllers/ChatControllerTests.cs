using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewJira.Application.DTOs.Chat;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Controllers.Chat;
using NewJira.Domain.Entities;

namespace NewJira.UnitTests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatRepository> _repository = new();

    [Fact]
    public async Task GetRooms_ReturnsUnauthorized_WhenUserClaimIsMissing()
    {
        var result = await CreateController().GetRooms();

        Assert.IsType<UnauthorizedObjectResult>(result);
        _repository.Verify(repository => repository.GetRoomsForUserAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateDirectRoom_ReturnsBadRequest_WhenUserRequestsRoomWithSelf()
    {
        var controller = CreateController(("Id", "8"));

        var result = await controller.CreateDirectRoom(new CreateDirectRoomDto { UserId = 8 });

        Assert.IsType<BadRequestObjectResult>(result);
        _repository.Verify(repository => repository.GetDirectRoomAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateDirectRoom_ReusesExistingRoom_WhenOneExists()
    {
        var existingRoom = new ChatRoom { Id = 10 };
        _repository.Setup(repository => repository.GetDirectRoomAsync(8, 9))
            .ReturnsAsync(existingRoom);
        var controller = CreateController(("Id", "8"));

        var result = await controller.CreateDirectRoom(new CreateDirectRoomDto { UserId = 9 });

        Assert.IsType<OkObjectResult>(result);
        _repository.Verify(repository => repository.CreateDirectRoomAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetMessages_ReturnsForbidden_WhenUserIsNotRoomMember()
    {
        _repository.Setup(repository => repository.IsMemberAsync(10, 8))
            .ReturnsAsync(false);
        var controller = CreateController(("Id", "8"));

        var result = await controller.GetMessages(10);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        _repository.Verify(repository => repository.GetHistoryAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SendMessage_SavesMessageWithAuthenticatedSender_WhenUserIsRoomMember()
    {
        _repository.Setup(repository => repository.IsMemberAsync(10, 8))
            .ReturnsAsync(true);
        _repository.Setup(repository => repository.AddMessageAsync(It.IsAny<ChatMessage>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        ChatMessage? addedMessage = null;
        _repository.Setup(repository => repository.AddMessageAsync(It.IsAny<ChatMessage>()))
            .Callback<ChatMessage>(message => addedMessage = message)
            .Returns(Task.CompletedTask);
        var controller = CreateController(("Id", "8"));

        var result = await controller.SendMessage(10, new SendMessageDto { Content = "Hello" });

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(addedMessage);
        Assert.Equal(10, addedMessage.RoomId);
        Assert.Equal(8, addedMessage.SenderId);
        Assert.Equal("Hello", addedMessage.Content);
        _repository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateGroupRoom_ReturnsBadRequest_WhenFewerThanTwoMembersAreProvided()
    {
        var controller = CreateController(("Id", "8"));

        var result = await controller.CreateGroupRoom(new CreateGroupRoomDto
        {
            Name = "Team",
            MemberIds = [9]
        });

        Assert.IsType<BadRequestObjectResult>(result);
        _repository.Verify(repository => repository.CreateGroupRoomAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<List<int>>()), Times.Never);
    }

    private ChatController CreateController(params (string Type, string Value)[] claims)
    {
        return new ChatController(_repository.Object)
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
