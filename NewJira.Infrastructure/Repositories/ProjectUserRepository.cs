using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace NewJira.Infrastructure.Repositories;

public class ProjectUserRepository : IProjectUserRepository
{
    private readonly JiraDbContext _context;

    public ProjectUserRepository(JiraDbContext context) => this._context = context;

    public async Task<IEnumerable<ProjectUser>> GetAllProjectUsersAsync()
    {
        return await this._context.Set<ProjectUser>()
            .Include(pu => pu.Member)
            .Include(pu => pu.Project)
            .ToListAsync();
    }

    public async Task<ProjectUser?> GetByIdsAsync(int membersId, int projectsId)
    {
        // Sử dụng FindAsync với cả 2 khóa chính ghép
        return await this._context.Set<ProjectUser>().FindAsync(membersId, projectsId);
    }

    public async Task AddProjectUserAsync(ProjectUser projectUser)
    {
        await this._context.Set<ProjectUser>().AddAsync(projectUser);
        await this._context.SaveChangesAsync();
    }

    public async Task DeleteProjectUserAsync(int membersId, int projectsId)
    {
        var projectUser = await GetByIdsAsync(membersId, projectsId);
        if (projectUser == null)
            return;

        this._context.Set<ProjectUser>().Remove(projectUser);
        await this._context.SaveChangesAsync();
    }
}