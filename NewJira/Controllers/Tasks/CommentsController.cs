using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Comment;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;

namespace NewJira.Controllers.Comments
{
    [Route("api/projects/comments/{projectId}/{taskId}")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;

        public CommentsController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetCommentsByTaskId([FromRoute] CommentRequest request)
        {
            var comments = await _commentRepository.GetCommentsByTaskIdAsync(request.ProjectId, request.TaskId);
            var response = comments.Select(MapToResponse);
            return Ok(new ResponseResultSuccess<object>("Lấy danh sách bình luận thành công", response));
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetCommentById([FromRoute] CommentRequest request, int id)
        {
            var comment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            if (comment == null)
                return NotFound(new ResponseResultError<object>("Không tìm thấy bình luận trong task này!"));
            return Ok(new ResponseResultSuccess<object>("Lấy thông tin bình luận thành công", MapToResponse(comment)));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateComment([FromRoute] CommentRequest request, [FromBody] CreateCommentDto model)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new ResponseResultError<object>("Token không hợp lệ!"));

            var comment = new Comment
            {
                ContentComment = model.Content,
                TaskItemId = request.TaskId,
                UserId = currentUserId.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(comment);
            var created = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, comment.Id);
            return Ok(new ResponseResultSuccess<object>("Thêm bình luận thành công", MapToResponse(created ?? comment)));
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateComment([FromRoute] CommentRequest request, int id, [FromBody] UpdateCommentDto model)
        {
            var comment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            if (comment == null)
                return NotFound(new ResponseResultError<object>("Không tìm thấy bình luận cần cập nhật."));

            // Chỉ người tạo bình luận (hoặc người có quyền task.update.all) mới sửa được
            if (!CanModifyComment(comment))
                return StatusCode(403, new ResponseResultError<object>("Bạn không có quyền sửa bình luận này!"));

            comment.ContentComment = model.Content;
            await _commentRepository.UpdateCommentAsync(comment);

            var updated = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật bình luận thành công", MapToResponse(updated ?? comment)));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] CommentRequest request, int id)
        {
            var comment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            if (comment == null)
                return NotFound(new ResponseResultError<object>("Không tìm thấy bình luận cần xóa."));

            if (!CanModifyComment(comment))
                return StatusCode(403, new ResponseResultError<object>("Bạn không có quyền xóa bình luận này!"));

            await _commentRepository.DeleteCommentAsync(comment);
            return Ok(new ResponseResultSuccess<object>("Xóa bình luận thành công"));
        }

        // --- HỖ TRỢ ---
        private int? GetCurrentUserId()
        {
            // SỬA: claim ngắn "Id" (khớp MapInboundClaims = false), không phải ClaimTypes.NameIdentifier
            var claim = User.FindFirst("Id")?.Value;
            return int.TryParse(claim, out var userId) ? userId : null;
        }

        private bool CanModifyComment(Comment comment)
        {
            var userId = GetCurrentUserId();
            return (userId.HasValue && comment.UserId == userId.Value)
                || User.HasClaim("perm", "task.update.all");
        }

        private CommentResponseDto MapToResponse(Comment c) => new()
        {
            Id = c.Id,
            Content = c.ContentComment,
            TaskId = c.TaskItemId,
            CreatedAt = c.CreatedAt,
            User = c.User == null ? null : new UserMemberListResponseDto
            {
                Id = c.User.Id,
                Name = c.User.Name ?? string.Empty,
                Avatar = c.User.Avatar ?? string.Empty
            }
        };
    }
}