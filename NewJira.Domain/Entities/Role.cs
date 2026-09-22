namespace NewJira.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public ICollection<PermissionRole> PermissionRoles { get; set; } = new List<PermissionRole>();
}