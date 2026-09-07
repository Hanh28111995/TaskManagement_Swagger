using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewJira.Controllers.Metadata
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusRepository _statusRepository;

        public StatusController(IStatusRepository statusRepository)
        {
            _statusRepository = statusRepository;
        }

        // 1. Lấy danh sách toàn bộ Status (Yêu cầu đăng nhập)
        [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllStatus()
        {
            var statuses = await _statusRepository.GetAllStatusAsync();
            return Ok(new { statusCode = 200, message = "Lấy danh sách trạng thái thành công", content = statuses });
        }

        // 3. Tạo mới Status (CHỈ ADMIN MỚI ĐƯỢC PHÉP)
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateStatus([FromBody] CreateStatusDto model)
        {            
            var newStatus = new Status
            {
                StatusName = model.StatusName,
                Alias = model.Alias
            };

            await _statusRepository.AddStatusAsync(newStatus);
            return Ok(new { statusCode = 200, message = "Tạo trạng thái thành công", content = newStatus });
        }

        // 4. Cập nhật Status (CHỈ ADMIN MỚI ĐƯỢC PHÉP)
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.StatusName))
            {
                return BadRequest(new { statusCode = 400, message = "Tên trạng thái cập nhật không được để trống!" });
            }

            var existingStatus = await _statusRepository.GetStatusByIdAsync(id);
            if (existingStatus == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy trạng thái để cập nhật!" });
            }

            existingStatus.StatusName = model.StatusName;
            existingStatus.Alias = model.Alias;

            await _statusRepository.UpdateStatusAsync(existingStatus);
            return Ok(new { statusCode = 200, message = "Cập nhật trạng thái thành công", content = existingStatus });
        }

        // 5. Xóa Status (CHỈ ADMIN MỚI ĐƯỢC PHÉP)
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var existingStatus = await _statusRepository.GetStatusByIdAsync(id);
            if (existingStatus == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy trạng thái để xóa!" });
            }

            await _statusRepository.DeleteStatusAsync(id);
            return Ok(new { statusCode = 200, message = "Xóa trạng thái thành công" });
        }

        public class CreateStatusDto
        {
            [Required(ErrorMessage = "Tên trạng thái không được để trống")]
            public string StatusName { get; set; } = string.Empty;
            [Required(ErrorMessage = "Tên trạng thái không được để trống")]
            public string Alias { get; set; } = string.Empty;
        }

        public class UpdateStatusDto
        {
            [Required(ErrorMessage = "Tên trạng thái không được để trống")]
            public string StatusName { get; set; } = string.Empty;
            [Required(ErrorMessage = "Tên trạng thái không được để trống")]
            public string Alias { get; set; } = string.Empty;
        }
    }
}