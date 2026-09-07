using NewJira.Application.DTOs.Auth; // Hoặc namespace chứa UserMemberListResponseDto của bạn

#nullable enable
namespace NewJira.Application.DTOs.Comment;

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Thông tin người dùng bình luận được nhúng gọn gàng vào đây
    public UserMemberListResponseDto? User { get; set; }
}