using System.ComponentModel.DataAnnotations;

#nullable enable
namespace NewJira.Domain.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Avatar { get; set; }

    public string? Password { get; set; }

    public string? Roles { get; set; }
}