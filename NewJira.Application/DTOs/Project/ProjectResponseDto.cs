
using NewJira.Application.DTOs.Auth;
using NewJira.Application.DTOs.Task;
using System.Collections.Generic;

#nullable enable
namespace NewJira.Application.DTOs.Project;

public class ProjectResponseDto
{
    public int Id { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public UserMemberListResponseDto? Creator { get; set; }

    public List<UserMemberListResponseDto> Members { get; set; } = new List<UserMemberListResponseDto>();

     public List<TaskListItemDto> Tasks { get; set; } = new List<TaskListItemDto>();
}

public class ProjectListResponseDto
{
    public int Id { get; set; }

    public string ProjectName { get; set; } = string.Empty;    

    public int CategoryId { get; set; }

    public int CreatorId { get; set; }

}
