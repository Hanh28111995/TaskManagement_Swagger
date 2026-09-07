using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace NewJira.Infrastructure.Repositories
{
    public class StatusRepository : IStatusRepository
    {
        private readonly JiraDbContext _context;

        public StatusRepository(JiraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Status>> GetAllStatusAsync()
        {
            return await _context.Statuses.ToListAsync();
        }

        public async Task<Status?> GetStatusByIdAsync(int id)
        {
            return await _context.Statuses.FindAsync(id);
        }

        public async Task AddStatusAsync(Status status)
        {
            await _context.Statuses.AddAsync(status);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(Status status)
        {
            _context.Statuses.Update(status);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStatusAsync(int id)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
                return;

            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();
        }
    }
}