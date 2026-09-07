using System.ComponentModel.DataAnnotations;
namespace NewJira.Application.DTOs.Comment;

public class CreateCommentDto
{
    [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phải chỉ định UserId cho bình luận")]
    public int UserId { get; set; }
}

public class UpdateCommentDto
{
    [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phải chỉ định UserId cho bình luận")]
    public int UserId { get; set; }
}