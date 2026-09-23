using System.ComponentModel.DataAnnotations;

namespace NewJira.Application.DTOs.Role;

public class RoleRequestDto
{
    [Required(ErrorMessage = "Tên vai trò không được để trống")]
    public string RoleName { get; set; } = string.Empty;

    public string RoleDescription { get; set; } = string.Empty;
}

public class RoleResponseDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();   
}