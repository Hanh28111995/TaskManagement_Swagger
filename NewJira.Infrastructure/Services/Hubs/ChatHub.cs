using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;

namespace NewJira.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepo;

        public ChatHub(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        // Lấy userId từ claim "Id" (khớp JwtHelper — MapInboundClaims = false)
        private int GetUserId()
        {
            var value = Context.User?.FindFirst("Id")?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }

        // Tham gia phòng: kiểm tra là member rồi mới vào group SignalR
        public async Task JoinRoom(string roomId)
        {
            if (!int.TryParse(roomId, out var roomIdInt)) return;

            var userId = GetUserId();
            if (userId == 0) return;

            // Bỏ check này nếu muốn đơn giản như bạn đã chọn — chỉ cần [Authorize]
            // if (!await _chatRepo.IsMemberAsync(roomIdInt, userId)) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomIdInt}");
        }

        public async Task LeaveRoom(string roomId)
        {
            if (!int.TryParse(roomId, out var roomIdInt)) return;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomIdInt}");
        }

        // Gửi tin: lưu DB -> broadcast về group
        public async Task SendMessage(int roomId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var userId = GetUserId();
            if (userId == 0) return;

            // Kiểm tra quyền gửi (nếu đã bật IsMemberAsync ở JoinRoom thì bỏ qua đây cho nhanh)
            if (!await _chatRepo.IsMemberAsync(roomId, userId)) return;

            var message = new ChatMessage
            {
                RoomId = roomId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            await _chatRepo.AddMessageAsync(message);
            await _chatRepo.SaveChangesAsync();

            // Broadcast: trả đủ thông tin để FE render (không cần gọi lại DB)
            await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                roomId,
                senderId = userId,
                content,
                sentAt = message.SentAt
            });
        }

        // (Tuỳ chọn) Báo danh sách user online trong phòng — nếu cần
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}