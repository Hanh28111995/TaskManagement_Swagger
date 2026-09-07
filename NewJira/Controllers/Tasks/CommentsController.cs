using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Comment;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.Security.Claims;

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

        // 1. Lấy danh sách bình luận theo ProjectId và TaskId
        [HttpGet("get-all")]
        public async Task<IActionResult> GetCommentsByTaskId([FromRoute] CommentRequest request)
        {
            var comments = await _commentRepository.GetCommentsByTaskIdAsync(request.ProjectId, request.TaskId);

            var response = comments.Select(c => new CommentResponseDto
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
            });

            return Ok(new ResponseResultSuccess<object>("Lấy danh sách bình luận thành công", response));
        }

        // 2. Lấy chi tiết một bình luận
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetCommentById([FromRoute] CommentRequest request, int id)
        {
            var comment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            if (comment == null)
            {
                return NotFound(new ResponseResultError<object>("Không tìm thấy bình luận trong task này!"));
            }

            var response = new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.ContentComment,
                TaskId = comment.TaskItemId,
                CreatedAt = comment.CreatedAt,
                User = comment.User == null ? null : new UserMemberListResponseDto
                {
                    Id = comment.User.Id,
                    Name = comment.User.Name ?? string.Empty,
                    Avatar = comment.User.Avatar ?? string.Empty
                }
            };

            return Ok(new ResponseResultSuccess<object>("Lấy thông tin bình luận thành công", response));
        }

        // 3. Tạo mới bình luận
        [HttpPost("create")]
        public async Task<IActionResult> CreateComment([FromRoute] CommentRequest request, [FromBody] CreateCommentDto model)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new ResponseResultError<object>("Token không hợp lệ!"));
            }

            var comment = new Comment
            {
                ContentComment = model.Content,
                TaskItemId = request.TaskId, // Lấy trực tiếp từ route chuẩn xác
                UserId = currentUserId.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(comment);

            var createdComment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, comment.Id);

            var response = new CommentResponseDto
            {
                Id = createdComment?.Id ?? comment.Id,
                Content = createdComment?.ContentComment ?? comment.ContentComment,
                TaskId = createdComment?.TaskItemId ?? comment.TaskItemId,
                CreatedAt = createdComment?.CreatedAt ?? comment.CreatedAt,
                User = createdComment?.User == null ? null : new UserMemberListResponseDto
                {
                    Id = createdComment.User.Id,
                    Name = createdComment.User.Name ?? string.Empty,
                    Avatar = createdComment.User.Avatar ?? string.Empty
                }
            };

            return Ok(new ResponseResultSuccess<object>("Thêm bình luận thành công", response));
        }

        // 4. Cập nhật bình luận
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateComment([FromRoute] CommentRequest request, int id, [FromBody] UpdateCommentDto model)
        {
            var existingComment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            if (existingComment == null)
            {
                return NotFound(new ResponseResultError<object>("Không tìm thấy bình luận cần cập nhật."));
            }

            existingComment.ContentComment = model.Content;
            await _commentRepository.UpdateCommentAsync(existingComment);

            var updatedComment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);

            var response = new CommentResponseDto
            {
                Id = updatedComment!.Id,
                Content = updatedComment.ContentComment,
                TaskId = updatedComment.TaskItemId,
                CreatedAt = updatedComment.CreatedAt,
                User = updatedComment.User == null ? null : new UserMemberListResponseDto
                {
                    Id = updatedComment.User.Id,
                    Name = updatedComment.User.Name ?? string.Empty,
                    Avatar = updatedComment.User.Avatar ?? string.Empty
                }
            };

            return Ok(new ResponseResultSuccess<object>("Cập nhật bình luận thành công", response));
        }

        // 5. Xóa bình luận
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] CommentRequest request, int id)
        {
            var comment = await _commentRepository.GetCommentByIdAndTaskAsync(request.ProjectId, request.TaskId, id);
            if (comment == null)
            {
                return NotFound(new ResponseResultError<object>("Không tìm thấy bình luận cần xóa."));
            }

            await _commentRepository.DeleteCommentAsync(comment);
            return Ok(new ResponseResultSuccess<object>("Xóa bình luận thành công"));
        }

        // --- HÀM HỖ TRỢ ---
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var userId) ? userId : null;
        }
    }
}