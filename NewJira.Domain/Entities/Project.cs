
using System.ComponentModel.DataAnnotations;
namespace NewJira.Domain.Entities;

public class Project
{
    [Key]
    public int Id { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int CreatorId { get; set; }

    public User? Creator { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = (ICollection<TaskItem>)new List<TaskItem>();

    public ICollection<ProjectUser> ProjectUsers { get; set; } = (ICollection<ProjectUser>)new List<ProjectUser>();
}
