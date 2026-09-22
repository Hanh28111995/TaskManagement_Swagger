using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewJira.Controllers.Metadata
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriorityController : ControllerBase
    {
        private readonly IPriorityRepository _priorityRepository;

        public PriorityController(IPriorityRepository priorityRepository)
        {
            _priorityRepository = priorityRepository;
        }
        
        [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPriorities()
        {
            var priorities = await _priorityRepository.GetAllPrioritiesAsync();
            return Ok(new { statusCode = 200, message = "Lấy danh sách độ ưu tiên thành công", content = priorities });
        }


        [Authorize(Policy = "user.manage")]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePriority([FromBody] PriorityDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PriorityName))
            {
                return BadRequest(new { statusCode = 400, message = "Tên độ ưu tiên không được để trống!" });
            }

            var newPriority = new Priority
            {
                PriorityName = model.PriorityName,
                PriorityRank = model.PriorityRank
            };

            await _priorityRepository.AddPriorityAsync(newPriority);
            return Ok(new { statusCode = 200, message = "Tạo độ ưu tiên thành công", content = newPriority });
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePriority(int id, [FromBody] PriorityDto model)
        {
            var existingPriority = await _priorityRepository.GetPriorityByIdAsync(id);
            if (existingPriority == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy độ ưu tiên để cập nhật!" });
            }

            existingPriority.PriorityName = model.PriorityName;
            existingPriority.PriorityRank = model.PriorityRank;

            await _priorityRepository.UpdatePriorityAsync(existingPriority);
            return Ok(new { statusCode = 200, message = "Cập nhật độ ưu tiên thành công", content = existingPriority });
        }

        [Authorize(Policy = "user.manage")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePriority(int id)
        {
            var existingPriority = await _priorityRepository.GetPriorityByIdAsync(id);
            if (existingPriority == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy độ ưu tiên để xóa!" });
            }

            await _priorityRepository.DeletePriorityAsync(id);
            return Ok(new { statusCode = 200, message = "Xóa độ ưu tiên thành công" });
        }

        // --- DTO NỘI BỘ ---
        public class PriorityDto
        {
            [Required(ErrorMessage = "Tên độ ưu tiên không được để trống")]
            public string PriorityName { get; set; } = string.Empty;

            public int PriorityRank { get; set; }
        }
    }
}