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
    public class PriorityRepository : IPriorityRepository
    {
        private readonly JiraDbContext _context;

        public PriorityRepository(JiraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Priority>> GetAllPrioritiesAsync()
        {
            return await _context.Priorities.ToListAsync();
        }

        public async Task<Priority?> GetPriorityByIdAsync(int id)
        {
            return await _context.Priorities.FindAsync(id);
        }

        public async Task AddPriorityAsync(Priority priority)
        {
            await _context.Priorities.AddAsync(priority);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePriorityAsync(Priority priority)
        {
            _context.Priorities.Update(priority);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePriorityAsync(int id)
        {
            var priority = await _context.Priorities.FindAsync(id);
            if (priority == null)
                return;

            _context.Priorities.Remove(priority);
            await _context.SaveChangesAsync();
        }
    }
}