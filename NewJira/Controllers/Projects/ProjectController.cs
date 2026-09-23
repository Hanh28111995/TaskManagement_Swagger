using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Auth; 
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Project;
using NewJira.Application.DTOs.Task;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;

namespace NewJira.Controllers.Projects;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public ProjectsController(IProjectRepository projectRepository, ITaskRepository taskRepository)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
    }

    [Authorize(Policy = "project.manage")]
    [HttpPost("create-project")]
    public async Task<IActionResult> CreateProject([FromBody] CreateUpdateProjectDto model)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null || currentUserId.Value != model.CreatorId) return Unauthorized(new ResponseResultError<object>("Token không hợp lệ!"));        

        var creatorId = currentUserId.Value;

        var project = new Project
        {
            ProjectName = model.ProjectName,
            Description = model.Description,
            CategoryId = model.CategoryId,
            CreatorId = creatorId
        };
        
        await _projectRepository.AddProjectAsync(project);

        // Lấy lại dự án vừa tạo (kèm thông tin Creator/Members) để map sang DTO trả về chuẩn
        var createdProject = await _projectRepository.GetProjectByIdAsync(project.Id);
        var projectDto = MapToProjectResponseDto(createdProject ?? project);

        return Ok(new ResponseResultSuccess<object>("Tạo dự án thành công", projectDto));
    }

    [HttpGet("get-project-detail/{id}")]
    public async Task<IActionResult> GetProjectDetail(int id)
    {
        var project = await _projectRepository.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound(new ResponseResultError<object>("Không tìm thấy dự án!"));
        }

        var projectDto = MapToProjectResponseDto(project);
        return Ok(new ResponseResultSuccess<object>("Lấy thông tin dự án thành công", projectDto));
    }

    [HttpGet("get-all-project")]
    public async Task<IActionResult> GetAllProject()
    {
        try 
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
                        
            var projectDtos = projects.Select(MapToProjectListResponseDto).ToList();

            return Ok(new ResponseResultSuccess<object>("Lấy danh sách dự án thành công", projectDtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                error = ex.Message, 
                inner = ex.InnerException?.Message 
            });
        }
    }

    [HttpDelete("delete-project/{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _projectRepository.GetProjectByIdAsync(id);
        if (project == null) return NotFound(new ResponseResultError<object>("Không tìm thấy dự án để xóa!"));
        if (!CanManageProject(project)) return Forbid();

        await _projectRepository.DeleteProjectAsync(id);
        return Ok(new ResponseResultSuccess<object>("Xóa dự án thành công"));
    }

    [HttpPut("update-project/{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] CreateUpdateProjectDto model)
    {
        var project = await _projectRepository.GetProjectByIdAsync(id);
        if (project == null) return NotFound(new ResponseResultError<object>("Không tìm thấy dự án!"));
        if (!CanManageProject(project)) return Forbid();

        project.ProjectName = model.ProjectName;
        project.Description = model.Description;
        project.CategoryId = model.CategoryId;
        
        await _projectRepository.UpdateProjectAsync(project);

        // Lấy lại thông tin sau khi update để trả về DTO chuẩn
        var updatedProject = await _projectRepository.GetProjectByIdAsync(id);
        var projectDto = MapToProjectResponseDto(updatedProject ?? project);

        return Ok(new ResponseResultSuccess<object>("Cập nhật dự án thành công", projectDto));
    }


    // --- CÁC HÀM HỖ TRỢ (HELPER METHODS) ---

    private bool CanManageProject(Project project)
    {
        return User.HasClaim("perm", "project.manage")
            || project.CreatorId == GetCurrentUserId();
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst("Id")?.Value;
        return int.TryParse(claim, out var userId) ? userId : null;
    }

    // Hàm tiện ích giúp ánh xạ từ Entity Project sang ProjectResponseDto chuẩn xác
    private ProjectResponseDto MapToProjectResponseDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            Description = project.Description,
            CategoryId = project.CategoryId,
            Creator = project.Creator != null ? new UserMemberListResponseDto
            {
                Id = project.Creator.Id,
                Name = project.Creator.Name,
                Avatar = project.Creator.Avatar ?? string.Empty
            } : null,
            Members = project.ProjectUsers?
                .Where(pu => pu.Member != null)
                .Select(pu => new UserMemberListResponseDto
                {
                    Id = pu.Member!.Id,
                    Name = pu.Member.Name,
                    Avatar = pu.Member.Avatar ?? string.Empty
                }).ToList() ?? new List<UserMemberListResponseDto>(),

            // 👇 MAP SANG TASKLISTITEMDTO CHUẨN XÁC 👇
            Tasks = project.Tasks?
                .Select(t => new TaskListItemDto
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    ProjectId = t.ProjectId,
                    StatusId = t.StatusId,
                    StatusName = t.Status?.StatusName, // Đảm bảo bên Repository đã .Include(t => t.Status)
                    PriorityId = t.PriorityId,
                    PriorityName = t.Priority?.PriorityName, // Đảm bảo đã .Include(t => t.Priority)
                    TaskTypeId = t.TaskTypeId,
                    TaskTypeName = t.TaskType?.TaskTypeName, // Đảm bảo đã .Include(t => t.TaskType)
                    Assignee = t.Assignee != null ? new UserMemberListResponseDto
                    {
                        Id = t.Assignee.Id,
                        Name = t.Assignee.Name,
                        Avatar = t.Assignee.Avatar ?? string.Empty
                    } : null
                }).ToList() ?? new List<TaskListItemDto>()
        };
    }


    private ProjectListResponseDto MapToProjectListResponseDto(Project project)
    {
        return new ProjectListResponseDto
        {
            Id = project.Id,
            ProjectName = project.ProjectName,         
            CategoryId = project.CategoryId,           
            CreatorId = project.CreatorId
        };
    }

}