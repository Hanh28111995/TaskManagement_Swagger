using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace NewJira.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]    
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RolesController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleRepository.GetAllRolesAsync();

            return Ok(new ResponseResultSuccess<object>(
                "Lấy danh sách vai trò thành công",
                roles));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);

            if (role == null)
            {
                return NotFound(new ResponseResultError<object>(
                    $"Không tìm thấy vai trò với ID {id}!"));
            }

            return Ok(new ResponseResultSuccess<object>(
                "Lấy thông tin vai trò thành công",
                role));
        }

        [Authorize(Policy = "user.manage")]
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] Role role)
        {
            if (role == null)
            {
                return BadRequest(new ResponseResultError<object>(
                    "Dữ liệu vai trò không hợp lệ."));
            }

            await _roleRepository.AddRoleAsync(role);

            // Trả về kết quả kèm theo route trỏ tới bản ghi vừa tạo
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, new ResponseResultSuccess<object>(
                "Thêm vai trò thành công",
                role));
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            if (role == null || id != role.Id)
            {
                return BadRequest(new ResponseResultError<object>(
                    "ID vai trò không khớp hoặc dữ liệu không hợp lệ."));
            }

            var existingRole = await _roleRepository.GetRoleByIdAsync(id);
            if (existingRole == null)
            {
                return NotFound(new ResponseResultError<object>(
                    $"Không tìm thấy vai trò với ID {id} để cập nhật!"));
            }

            await _roleRepository.UpdateRoleAsync(role);

            return Ok(new ResponseResultSuccess<object>(
                "Cập nhật vai trò thành công"));
        }

        [Authorize(Policy = "user.manage")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new ResponseResultError<object>(
                    $"Không tìm thấy vai trò với ID {id} để xóa!"));
            }

            await _roleRepository.DeleteRoleAsync(id);

            return Ok(new ResponseResultSuccess<object>(
                "Xóa vai trò thành công"));
        }
    }
}