using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;

namespace NewJira.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly JiraDbContext _context;

        public CommentRepository(JiraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(int projectId, int taskId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TaskItemId == taskId && _context.TaskItems.Any(t => t.Id == taskId && t.ProjectId == projectId))
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAndTaskAsync(int projectId, int taskId, int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && c.TaskItemId == taskId && _context.TaskItems.Any(t => t.Id == taskId && t.ProjectId == projectId));
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(Comment comment)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveCommentChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}