using Microsoft.EntityFrameworkCore;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
public class RoleRepository : IRoleRepository
{
    private readonly JiraDbContext _context;
    public RoleRepository(JiraDbContext context) => _context = context;

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
        => await _context.Roles
            .Include(r => r.PermissionRoles)
                .ThenInclude(pr => pr.Permission)
            .ToListAsync();

    public async Task<Role?> GetRoleByIdAsync(int id)
        => await _context.Roles
            .Include(r => r.PermissionRoles)
                .ThenInclude(pr => pr.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddRoleAsync(Role role) { await _context.Roles.AddAsync(role); await _context.SaveChangesAsync(); }
    public async Task UpdateRoleAsync(Role role) { _context.Roles.Update(role); await _context.SaveChangesAsync(); }

    public async Task DeleteRoleAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return;
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    public async Task AssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds)
    {
        var existing = _context.PermissionRoles.Where(pr => pr.RoleId == roleId);
        _context.PermissionRoles.RemoveRange(existing);

        foreach (var pid in permissionIds.Distinct())
            _context.PermissionRoles.Add(new PermissionRole { RoleId = roleId, PermissionId = pid });

        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}