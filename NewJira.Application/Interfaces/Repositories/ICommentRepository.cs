using System.Collections.Generic;
using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(int projectId, int taskId);
    Task<Comment?> GetCommentByIdAndTaskAsync(int projectId, int taskId, int id);    
    Task AddCommentAsync(Comment comment); // Chỉ cần nhận entity comment (vì đã gán sẵn TaskItemId)
    Task UpdateCommentAsync(Comment comment);
    Task DeleteCommentAsync(Comment comment); // Truyền trực tiếp entity hoặc dùng id tùy ý
    Task<bool> SaveCommentChangesAsync();
}