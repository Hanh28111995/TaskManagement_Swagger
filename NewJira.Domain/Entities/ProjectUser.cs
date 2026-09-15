using System.ComponentModel.DataAnnotations;
namespace NewJira.Domain.Entities;

public class ProjectUser
{    
    public int MembersId { get; set; }
    public User? Member { get; set; }
 
    public int ProjectsId { get; set; }
    public Project? Project { get; set; }
}