using Microsoft.EntityFrameworkCore;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Task;
using NewJira.Domain.Entities;
using NewJira.Infrastructure.Data;


public class TaskRepository : ITaskRepository
{
    private readonly JiraDbContext _context;

    public TaskRepository(JiraDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? userId, string role)
    {
        var query = _context.TaskItems.AsQueryable();
        if (role == "Member" && userId.HasValue)
            query = query.Where(t => t.AssigneeId == userId.Value);

        var filteredByStatus = await query
            .GroupBy(t => new { t.StatusId, t.Status!.StatusName })
            .Select(g => new StatusCountDto
            {
                StatusId = g.Key.StatusId,
                StatusName = g.Key.StatusName,
                Count = g.Count()
            })
            .ToListAsync();

        // ✅ InProgress: lọc ĐÚNG status đang làm (bỏ dấu !)
        var inProgressCount = filteredByStatus
            .Where(s => s.StatusName.ToLower().Contains("progress")
                     || s.StatusName.ToLower().Contains("doing"))
            .Sum(s => s.Count);

        // ✅ Completed: dùng Alias (đúng chuẩn seed của bạn)
        var completedByProject = await _context.Projects
            .GroupJoin(
                _context.TaskItems.Where(t => t.Status!.Alias != null
                                            && t.Status!.Alias.ToLower().Contains("done")),
                p => p.Id,
                t => t.ProjectId,
                (p, tasks) => new ProjectCompletedDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.ProjectName,
                    Completed = tasks.Count()
                })
            .ToListAsync();

        return new DashboardSummaryDto
        {            
            TotalProjects = await _context.Projects.CountAsync(),
            TotalTasks = filteredByStatus.Sum(s => s.Count),
            CompletedTasks = completedByProject.Sum(p => p.Completed),
            InProgressTasks = inProgressCount,
            ByStatus = filteredByStatus,          
        };
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksByMemberAsync(int memberId)
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Where(t => t.AssigneeId == memberId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksByStatusAsync(int statusId)
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Where(t => t.StatusId == statusId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksByStatusByMemberAsync(int statusId, int memberId)
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Where(t => t.StatusId == statusId && t.AssigneeId == memberId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetTaskByProjectIdAsync(int projectId)
    {
        return await _context.TaskItems
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskDetailByProjectIdAsync(int projectId, int taskId)
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
    }

    public async Task AddTaskAsync(int projectId, TaskItem taskItem)
    {
        taskItem.ProjectId = projectId; // Đảm bảo gán đúng projectId vào task
        await _context.TaskItems.AddAsync(taskItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTaskAsync(int projectId, TaskItem taskItem)
    {
        // Có thể bổ sung kiểm tra đúng projectId nếu cần thiết
        _context.TaskItems.Update(taskItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int projectId, int taskId)
    {
        var taskItem = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (taskItem == null) return;

        _context.TaskItems.Remove(taskItem);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SaveTaskChangesAsync() => await _context.SaveChangesAsync() > 0;

    // --- CÁC HÀM CẬP NHẬT RIÊNG LẺ KÈM PROJECTID ---

    public async Task<bool> UpdateTaskAssignmentAsync(int projectId, int taskId, TaskAssignmentDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (task == null) return false;

        task.AssigneeId = dto.AssigneeId;
        _context.TaskItems.Update(task);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTaskEstimateAsync(int projectId, int taskId, TaskEstimateDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (task == null) return false;

        task.EstimateHours = dto.EstimateHours;
        _context.TaskItems.Update(task);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTaskPriorityAsync(int projectId, int taskId, TaskPriorityDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (task == null) return false;

        task.PriorityId = dto.PriorityId;
        _context.TaskItems.Update(task);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTaskStatusAsync(int projectId, int taskId, TaskStatusDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (task == null) return false;

        task.StatusId = dto.StatusId;
        _context.TaskItems.Update(task);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTaskTimeTrackingAsync(int projectId, int taskId, TaskTimeTrackingDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (task == null) return false;

        task.TimeTrackingSpentHours = dto.TimeTrackingSpentHours;
        task.TimeTrackingRemainingHours = dto.TimeTrackingRemainingHours;
        _context.TaskItems.Update(task);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTaskTypeAsync(int projectId, int taskId, TaskTypeDto dto)
    {
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);
        if (task == null) return false;

        task.TaskTypeId = dto.TaskTypeId;
        _context.TaskItems.Update(task);
        return await _context.SaveChangesAsync() > 0;
    }
}