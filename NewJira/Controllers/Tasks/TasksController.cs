using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Task;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.DTOs.Project;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;

namespace NewJira.Controllers.Tasks
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;

        public TasksController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        // 1. Lấy danh sách task (Có phân quyền theo Role)
        [HttpGet("get-all-task")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskRepository.GetAllTasksAsync();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu là Member, chỉ lấy các task được phân công cho chính user đó
            if (role == "Member" && int.TryParse(userIdClaim, out int userId))
            {
                tasks = tasks.Where(t => t.AssigneeId == userId).ToList();
            }

            var taskDtos = tasks.Select(MapToTaskResponseDto).ToList();

            return Ok(new ResponseResultSuccess<object>("Lấy danh sách task thành công", taskDtos));
        }

        // 2. Lấy chi tiết task theo projectId và id
        [HttpGet("{projectId}/get-task-detail/{id}")]
        public async Task<IActionResult> GetTaskById(int projectId, int id)
        {
            var task = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (task == null)
            {
                return NotFound(new ResponseResultError<object>("Không tìm thấy task trong dự án này!"));
            }

            var taskDto = MapToTaskResponseDto(task);
            return Ok(new ResponseResultSuccess<object>("Lấy thông tin task thành công", taskDto));
        }

        // 3. Tạo mới task cho dự án (Tự động gán Assignee là user hiện tại nếu chưa có)
        [HttpPost("{projectId}/create-task")]
        public async Task<IActionResult> CreateTask(int projectId, [FromBody] CreateTaskDto model)
        {
            if (!model.AssigneeId.HasValue || model.AssigneeId == 0)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int currentUserId))
                {
                    model.AssigneeId = currentUserId;
                }
            }

            var newTask = new TaskItem
            {
                TaskName = model.TaskName,
                Description = model.Description,
                EstimateHours = model.EstimateHours,
                TimeTrackingSpentHours = model.TimeTrackingSpentHours,
                TimeTrackingRemainingHours = model.TimeTrackingRemainingHours,
                ProjectId = projectId,
                StatusId = model.StatusId,
                PriorityId = model.PriorityId,
                TaskTypeId = model.TaskTypeId,
                AssigneeId = model.AssigneeId
            };

            await _taskRepository.AddTaskAsync(projectId, newTask);

            var createdTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, newTask.Id);
            var taskDto = MapToTaskResponseDto(createdTask ?? newTask);

            return Ok(new ResponseResultSuccess<object>("Tạo task thành công", taskDto));
        }

        // 4. Cập nhật toàn bộ thông tin task
        [HttpPut("{projectId}/update-task/{id}")]
        public async Task<IActionResult> UpdateTask(int projectId, int id, [FromBody] CreateTaskDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null)
            {
                return NotFound(new ResponseResultError<object>("Không tìm thấy task để cập nhật!"));
            }

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            existingTask.TaskName = model.TaskName;
            existingTask.Description = model.Description;
            existingTask.EstimateHours = model.EstimateHours;
            existingTask.StatusId = model.StatusId;
            existingTask.PriorityId = model.PriorityId;
            existingTask.TaskTypeId = model.TaskTypeId;
            existingTask.AssigneeId = model.AssigneeId;

            await _taskRepository.UpdateTaskAsync(projectId, existingTask);

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            var taskDto = MapToTaskResponseDto(updatedTask ?? existingTask);

            return Ok(new ResponseResultSuccess<object>("Cập nhật task thành công", taskDto));
        }

        // 5. Cập nhật nhanh trạng thái Task
        [HttpPatch("{projectId}/update-task/{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int projectId, int id, [FromBody] TaskStatusDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            var success = await _taskRepository.UpdateTaskStatusAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật trạng thái task thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 6. Cập nhật nhanh người thực hiện (Assignee)
        [HttpPatch("{projectId}/update-task/{id}/assignee")]
        public async Task<IActionResult> UpdateTaskAssignee(int projectId, int id, [FromBody] TaskAssignmentDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            var success = await _taskRepository.UpdateTaskAssignmentAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Phân công công việc thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 7. Cập nhật nhanh mức độ ưu tiên (Priority)
        [HttpPatch("{projectId}/update-task/{id}/priority")]
        public async Task<IActionResult> UpdateTaskPriority(int projectId, int id, [FromBody] TaskPriorityDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            var success = await _taskRepository.UpdateTaskPriorityAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật mức độ ưu tiên thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 8. Cập nhật nhanh loại công việc (TaskType)
        [HttpPatch("{projectId}/update-task/{id}/task-type")]
        public async Task<IActionResult> UpdateTaskType(int projectId, int id, [FromBody] TaskTypeDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            var success = await _taskRepository.UpdateTaskTypeAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật loại công việc thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 9. Cập nhật nhanh thời gian ước lượng (Estimate Hours)
        [HttpPatch("{projectId}/update-task/{id}/estimate")]
        public async Task<IActionResult> UpdateTaskEstimate(int projectId, int id, [FromBody] TaskEstimateDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            var success = await _taskRepository.UpdateTaskEstimateAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật thời gian ước lượng thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 10. Cập nhật tiến độ thời gian (Time Tracking)
        [HttpPatch("{projectId}/update-task/{id}/time-tracking")]
        public async Task<IActionResult> UpdateTaskTimeTracking(int projectId, int id, [FromBody] TaskTimeTrackingDto model)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            var success = await _taskRepository.UpdateTaskTimeTrackingAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật time tracking thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 11. Xóa task
        [HttpDelete("{projectId}/delete-task/{id}")]
        public async Task<IActionResult> DeleteTask(int projectId, int id)
        {
            var existingTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            if (existingTask == null)
            {
                return NotFound(new ResponseResultError<object>("Không tìm thấy task để xóa!"));
            }

            if (!TryValidateTaskPermission(existingTask, out var errorResult)) return errorResult;

            await _taskRepository.DeleteTaskAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Xóa task thành công"));
        }

        // --- HÀM HỖ TRỢ KIỂM TRA QUYỀN (PERMISSION HELPER) ---
        private bool TryValidateTaskPermission(TaskItem task, out IActionResult errorResult)
        {
            errorResult = null;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Admin được phép thao tác trên mọi task mà không cần thỏa điều kiện Assignee
            if (role == "Admin")
            {
                return true;
            }

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                errorResult = Unauthorized(new ResponseResultError<object>("Không thể xác định danh tính người dùng!"));
                return false;
            }

            // Các role khác bắt buộc UserId phải khớp với AssigneeId của task
            if (task.AssigneeId != currentUserId)
            {
                errorResult = StatusCode(StatusCodes.Status403Forbidden,
                    new ResponseResultError<object>("Bạn không có quyền thực hiện thao tác này vì bạn không phải là người được phân công (Assignee)!"));
                return false;
            }

            return true;
        }

        // --- HÀM HỖ TRỢ ÁNH XẠ (MAPPER HELPER) ---
        private TaskResponseDto MapToTaskResponseDto(TaskItem task)
        {
            if (task == null) return new TaskResponseDto();

            return new TaskResponseDto
            {
                Id = task.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                EstimateHours = task.EstimateHours,
                TimeTrackingSpentHours = task.TimeTrackingSpentHours,
                TimeTrackingRemainingHours = task.TimeTrackingRemainingHours,                
                StatusId = task.StatusId,
                PriorityId = task.PriorityId,
                PriorityName = task.Priority?.PriorityName,
                TaskTypeId = task.TaskTypeId,
                TaskTypeName = task.TaskType?.TaskTypeName,
                Assignee = task.Assignee != null ? new UserMemberListResponseDto
                {
                    Id = task.Assignee.Id,
                    Name = task.Assignee.Name,
                    Avatar = task.Assignee.Avatar ?? string.Empty
                } : null,
                Project = task.Project != null ? new ProjectListResponseDto
                {
                    Id = task.Project.Id,
                    ProjectName = task.Project.ProjectName ?? string.Empty
                } : null
            };
        }
    }
}