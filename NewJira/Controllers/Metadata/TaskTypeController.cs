using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewJira.Controllers.Metadata
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskTypeController : ControllerBase
    {
        private readonly ITaskTypeRepository _taskTypeRepository;

        public TaskTypeController(ITaskTypeRepository taskTypeRepository)
        {
            _taskTypeRepository = taskTypeRepository;
        }
        
        [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTaskTypes()
        {
            var taskTypes = await _taskTypeRepository.GetAllTaskTypesAsync();
            return Ok(new { statusCode = 200, message = "Lấy danh sách loại công việc thành công", content = taskTypes });
        }       

        [Authorize(Policy = "user.manage")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTaskType([FromBody] CreateTaskTypeDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TaskTypeName))
            {
                return BadRequest(new { statusCode = 400, message = "Tên loại công việc không được để trống!" });
            }

            var newTaskType = new TaskType
            {
                TaskTypeName = model.TaskTypeName,
            };

            await _taskTypeRepository.AddTaskTypeAsync(newTaskType);
            return Ok(new { statusCode = 200, message = "Tạo loại công việc thành công", content = newTaskType });
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTaskType(int id, [FromBody] UpdateTaskTypeDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TaskTypeName))
            {
                return BadRequest(new { statusCode = 400, message = "Tên loại công việc cập nhật không được để trống!" });
            }

            var existingTaskType = await _taskTypeRepository.GetTaskTypeByIdAsync(id);
            if (existingTaskType == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy loại công việc để cập nhật!" });
            }

            existingTaskType.TaskTypeName = model.TaskTypeName;

            await _taskTypeRepository.UpdateTaskTypeAsync(existingTaskType);
            return Ok(new { statusCode = 200, message = "Cập nhật loại công việc thành công", content = existingTaskType });
        }

        [Authorize(Policy = "user.manage")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTaskType(int id)
        {
            var existingTaskType = await _taskTypeRepository.GetTaskTypeByIdAsync(id);
            if (existingTaskType == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy loại công việc để xóa!" });
            }

            await _taskTypeRepository.DeleteTaskTypeAsync(id);
            return Ok(new { statusCode = 200, message = "Xóa loại công việc thành công" });
        }

        // --- DTOs NỘI BỘ ---
        public class CreateTaskTypeDto
        {
            [Required(ErrorMessage = "Tên loại công việc không được để trống")]
            public string TaskTypeName { get; set; } = string.Empty;
        }

        public class UpdateTaskTypeDto
        {
            [Required(ErrorMessage = "Tên loại công việc không được để trống")]
            public string TaskTypeName { get; set; } = string.Empty;
        }
    }
}