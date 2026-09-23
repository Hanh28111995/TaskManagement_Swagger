using NewJira.Domain.Entities;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllRolesAsync();          // Include PermissionRoles
    Task<Role?> GetRoleByIdAsync(int id);                // Include PermissionRoles
    Task AddRoleAsync(Role role);
    Task UpdateRoleAsync(Role role);
    Task DeleteRoleAsync(int id);
    Task SaveChangesAsync();

    // Gán lại toàn bộ quyền cho role (xóa cũ, thêm mới)
    Task AssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds);
}