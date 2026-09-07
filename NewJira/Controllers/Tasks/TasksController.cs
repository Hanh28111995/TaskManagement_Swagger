using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Task;
using NewJira.Application.DTOs.Auth;
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

        // 1. Lấy danh sách task theo Project ID
        [HttpGet("get-all-task/{projectId}")]
        public async Task<IActionResult> GetAllTasks(int projectId)
        {
            var tasks = await _taskRepository.GetTaskByProjectIdAsync(projectId);
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

        // 3. Tạo mới task cho dự án
        [HttpPost("{projectId}/create-task")]
        public async Task<IActionResult> CreateTask(int projectId, [FromBody] CreateTaskDto model)
        {
            var newTask = new TaskItem
            {
                TaskName = model.TaskName,
                Description = model.Description,
                EstimateHours = model.EstimateHours,
                TimeTrackingSpentHours = model.TimeTrackingSpentHours,
                TimeTrackingRemainingHours = model.TimeTrackingRemainingHours,
                ProjectId = projectId, // Gán trực tiếp projectId từ route
                StatusId = model.StatusId,
                PriorityId = model.PriorityId,
                TaskTypeId = model.TaskTypeId,
                AssigneeId = model.AssigneeId
            };

            await _taskRepository.AddTaskAsync(projectId, newTask);

            // Lấy lại task vừa tạo kèm đầy đủ thông tin quan hệ
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
            var success = await _taskRepository.UpdateTaskStatusAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật trạng thái task thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 6. Cập nhật nhanh người thực hiện (Assignee)
        [HttpPatch("{projectId}/update-task/{id}/assignee")]
        public async Task<IActionResult> UpdateTaskAssignee(int projectId, int id, [FromBody] TaskAssignmentDto model)
        {
            var success = await _taskRepository.UpdateTaskAssignmentAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Phân công công việc thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 7. Cập nhật nhanh mức độ ưu tiên (Priority)
        [HttpPatch("{projectId}/update-task/{id}/priority")]
        public async Task<IActionResult> UpdateTaskPriority(int projectId, int id, [FromBody] TaskPriorityDto model)
        {
            var success = await _taskRepository.UpdateTaskPriorityAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật mức độ ưu tiên thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 8. Cập nhật nhanh loại công việc (TaskType)
        [HttpPatch("{projectId}/update-task/{id}/task-type")]
        public async Task<IActionResult> UpdateTaskType(int projectId, int id, [FromBody] TaskTypeDto model)
        {
            var success = await _taskRepository.UpdateTaskTypeAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật loại công việc thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 9. Cập nhật nhanh thời gian ước lượng (Estimate Hours)
        [HttpPatch("{projectId}/update-task/{id}/estimate")]
        public async Task<IActionResult> UpdateTaskEstimate(int projectId, int id, [FromBody] TaskEstimateDto model)
        {
            var success = await _taskRepository.UpdateTaskEstimateAsync(projectId, id, model);
            if (!success) return NotFound(new ResponseResultError<object>("Không tìm thấy task!"));

            var updatedTask = await _taskRepository.GetTaskDetailByProjectIdAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Cập nhật thời gian ước lượng thành công", MapToTaskResponseDto(updatedTask)));
        }

        // 10. Cập nhật tiến độ thời gian (Time Tracking)
        [HttpPatch("{projectId}/update-task/{id}/time-tracking")]
        public async Task<IActionResult> UpdateTaskTimeTracking(int projectId, int id, [FromBody] TaskTimeTrackingDto model)
        {
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

            await _taskRepository.DeleteTaskAsync(projectId, id);
            return Ok(new ResponseResultSuccess<object>("Xóa task thành công"));
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
                ProjectId = task.ProjectId,
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
                } : null
            };
        }
    }
}