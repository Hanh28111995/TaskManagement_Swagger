using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewJira.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public ChatRoom? Room { get; set; }
    public int SenderId { get; set; }
    public User? Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}