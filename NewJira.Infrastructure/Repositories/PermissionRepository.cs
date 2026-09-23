using Microsoft.EntityFrameworkCore;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using NewJira.Application.Interfaces.Repositories; 

namespace NewJira.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly JiraDbContext _context;
        public PermissionRepository(JiraDbContext context) => _context = context;

        public async Task<IEnumerable<Permission>> GetAllAsync() => await _context.Permissions.ToListAsync();
        public async Task<Permission?> GetByIdAsync(int id) => await _context.Permissions.FindAsync(id);
        public async Task AddAsync(Permission p) { await _context.Permissions.AddAsync(p); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var p = await _context.Permissions.FindAsync(id);
            if (p == null) return;
            _context.Permissions.Remove(p);
            await _context.SaveChangesAsync();
        }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
