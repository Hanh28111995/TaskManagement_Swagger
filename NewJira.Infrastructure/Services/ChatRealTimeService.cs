using Microsoft.AspNetCore.SignalR;
using NewJira.Application.Services;
using NewJira.Hubs;

namespace NewJira.Infrastructure.Services
{
    public class ChatRealtimeService : IChatRealtimeService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatRealtimeService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageNotificationAsync(int roomId, int senderId, string messageContent)
        {
            await _hubContext.Clients.Group($"Room_{roomId}")
                .SendAsync("ReceiveMessage", new
                {
                    RoomId = roomId,
                    SenderId = senderId,
                    Message = messageContent,
                    SentAt = DateTime.UtcNow
                });
        }
    }
}