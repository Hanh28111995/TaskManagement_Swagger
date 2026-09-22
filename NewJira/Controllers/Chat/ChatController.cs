using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Chat;

using NewJira.Application.Interfaces.Repositories;
using System.Security.Claims;

namespace NewJira.Controllers.Chat
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;

        public ChatController(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        private int GetUserId()
        {
            var value = User.FindFirst("Id")?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }

        private string GetRole()
        {
            return User.FindFirst("role")?.Value ?? "";
        }

        // 1. Danh sách phòng của user (DM + Group)
        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new ResponseResultError<object>("Không xác định được người dùng!"));

            var rooms = await _chatRepo.GetRoomsForUserAsync(userId);
            return Ok(new ResponseResultSuccess<object>("Danh sách phòng", rooms));
        }

        // 2. Mở/tạo phòng DM với user khác
        [HttpPost("rooms/direct")]
        public async Task<IActionResult> CreateDirectRoom([FromBody] CreateDirectRoomDto dto)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new ResponseResultError<object>("Không xác định được người dùng!"));
            if (dto.UserId <= 0 || dto.UserId == userId)
                return BadRequest(new ResponseResultError<object>("UserId không hợp lệ!"));

            // Tìm phòng DM đã có giữa 2 người, chưa có thì tạo
            var room = await _chatRepo.GetDirectRoomAsync(userId, dto.UserId);
            if (room == null)
            {
                room = await _chatRepo.CreateDirectRoomAsync(userId, dto.UserId);
            }

            return Ok(new ResponseResultSuccess<object>("Mở phòng trò chuyện thành công", room));
        }

        // 3. Tạo group chat — chỉ Manager/Admin
        [Authorize(Policy = "chat.group.create")]
        [HttpPost("rooms/group")]
        public async Task<IActionResult> CreateGroupRoom([FromBody] CreateGroupRoomDto dto)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new ResponseResultError<object>("Không xác định được người dùng!"));

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.MemberIds == null || dto.MemberIds.Count < 2)
                return BadRequest(new ResponseResultError<object>(
                    "Cần tên nhóm và ít nhất 2 thành viên!"));

            var room = await _chatRepo.CreateGroupRoomAsync(dto.Name, userId, dto.MemberIds);
            return Ok(new ResponseResultSuccess<object>("Tạo group chat thành công", room));
        }

        // 4. Lịch sử tin nhắn của phòng
        [HttpGet("rooms/{roomId}/messages")]
        public async Task<IActionResult> GetMessages(int roomId, int page = 1, int pageSize = 50)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new ResponseResultError<object>("Không xác định được người dùng!"));

            if (!await _chatRepo.IsMemberAsync(roomId, userId))
                return StatusCode(403, new ResponseResultError<object>("Bạn không phải thành viên phòng này!"));

            var messages = await _chatRepo.GetHistoryAsync(roomId, page, pageSize);
            return Ok(new ResponseResultSuccess<object>("Lịch sử tin nhắn", messages));
        }

        // 5. Gửi tin qua REST (dự phòng — chính vẫn qua SignalR)
        [HttpPost("rooms/{roomId}/messages")]
        public async Task<IActionResult> SendMessage(int roomId, [FromBody] SendMessageDto dto)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new ResponseResultError<object>("Không xác định được người dùng!"));
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new ResponseResultError<object>("Nội dung tin nhắn trống!"));

            if (!await _chatRepo.IsMemberAsync(roomId, userId))
                return StatusCode(403, new ResponseResultError<object>("Bạn không phải thành viên phòng này!"));

            var message = new NewJira.Domain.Entities.ChatMessage
            {
                RoomId = roomId,
                SenderId = userId,
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            await _chatRepo.AddMessageAsync(message);
            await _chatRepo.SaveChangesAsync();

            return Ok(new ResponseResultSuccess<object>("Gửi tin nhắn thành công", message));
        }
    }
}