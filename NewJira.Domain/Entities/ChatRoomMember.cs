using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewJira.Domain.Entities;

public class ChatRoomMember
{
    public int RoomId { get; set; }
    public ChatRoom? Room { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}