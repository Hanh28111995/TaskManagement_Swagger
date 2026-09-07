using System.ComponentModel.DataAnnotations;
namespace NewJira.Domain.Entities;

public class ProjectUser
{
    [Key]
    public int MembersId { get; set; }
    public User? Member { get; set; }
    [Key]
    public int ProjectsId { get; set; }
    public Project? Project { get; set; }
}