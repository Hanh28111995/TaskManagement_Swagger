using Microsoft.AspNetCore.SignalR;

namespace NewJira.Hubs
{
    public class ChatHub : Hub
    {
        // Cho phép người dùng tham gia vào phòng chat của một Project hoặc một Channel riêng
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }

        // Rời khỏi phòng chat
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }
    }
}