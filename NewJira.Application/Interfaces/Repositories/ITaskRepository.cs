using NewJira.Application.DTOs.Task;
using NewJira.Domain.Entities;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllTasksAsync();    
    Task<IEnumerable<TaskItem>> GetTaskByProjectIdAsync(int projectId);
    Task<TaskItem?> GetTaskDetailByProjectIdAsync(int projectId, int taskId);
    Task AddTaskAsync(int projectId, TaskItem taskItem);
    Task UpdateTaskAsync(int projectId, TaskItem taskItem);
    Task DeleteTaskAsync(int projectId, int taskId); // Cần cả projectId khi xóa
    Task<bool> SaveTaskChangesAsync();

    // Các hàm cập nhật riêng lẻ theo DTO cần kèm theo projectId để bảo mật và chính xác
    Task<bool> UpdateTaskAssignmentAsync(int projectId, int taskId, TaskAssignmentDto dto);
    Task<bool> UpdateTaskEstimateAsync(int projectId, int taskId, TaskEstimateDto dto);
    Task<bool> UpdateTaskPriorityAsync(int projectId, int taskId, TaskPriorityDto dto);
    Task<bool> UpdateTaskStatusAsync(int projectId, int taskId, TaskStatusDto dto);
    Task<bool> UpdateTaskTimeTrackingAsync(int projectId, int taskId, TaskTimeTrackingDto dto);
    Task<bool> UpdateTaskTypeAsync(int projectId, int taskId, TaskTypeDto dto);
}