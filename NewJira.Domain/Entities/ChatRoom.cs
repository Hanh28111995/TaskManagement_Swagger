using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewJira.Domain.Entities;

public class ChatRoom
{
    public int Id { get; set; }
    public string? Name { get; set; }                 // null với DM, có tên với Group
    public string Type { get; set; } = "Direct";      // "Direct" | "Group"
    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}