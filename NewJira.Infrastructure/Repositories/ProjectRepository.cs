using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace NewJira.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly JiraDbContext _context;

    public ProjectRepository(JiraDbContext context) => this._context = context;

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await this._context.Projects
            .Include(p => p.Creator)
            .Include(p => p.Tasks)
            .Include(p => p.ProjectUsers) // Sửa lỗi ép kiểu bằng cách dùng cú pháp lambda trực tiếp của EF Core
            .ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await this._context.Projects
            .Include(p => p.Creator)
            .Include(p => p.Tasks)
            .Include(p => p.ProjectUsers)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddProjectAsync(Project project)
    {
        EntityEntry<Project> entityEntry = await this._context.Projects.AddAsync(project);
        int num = await this._context.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(Project project)
    {
        this._context.Projects.Update(project);
        int num = await this._context.SaveChangesAsync();
    }

    public async Task DeleteProjectAsync(int id)
    {
        Project? project = await this._context.Projects.FindAsync(id);
        if (project == null)
            return;
        this._context.Projects.Remove(project);
        int num = await this._context.SaveChangesAsync();
    }

    public async Task<bool> SaveProjectChangesAsync() => await this._context.SaveChangesAsync() > 0;
}