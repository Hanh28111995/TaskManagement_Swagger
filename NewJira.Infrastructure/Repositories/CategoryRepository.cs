using Microsoft.EntityFrameworkCore;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace NewJira.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly JiraDbContext _context;

        public CategoryRepository(JiraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllProjectCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetProjectCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task AddProjectCategoryAsync(Category projectCategory)
        {
            await _context.Categories.AddAsync(projectCategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProjectCategoryAsync(Category projectCategory)
        {
            _context.Categories.Update(projectCategory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}