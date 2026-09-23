using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Common;
using NewJira.Application.DTOs.Role;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;

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

        // 1. Danh sách vai trò (kèm mã quyền) — mọi user đã đăng nhập
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return Ok(new ResponseResultSuccess<object>(
                "Lấy danh sách vai trò thành công",
                roles.Select(MapToResponse)));
        }

        // 2. Chi tiết vai trò
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new ResponseResultError<object>($"Không tìm thấy vai trò với ID {id}!"));

            return Ok(new ResponseResultSuccess<object>("Lấy thông tin vai trò thành công", MapToResponse(role)));
        }

        // 3. Tạo vai trò
        [Authorize(Policy = "role.assign")]
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] RoleRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new ResponseResultError<object>("Tên vai trò không được để trống!"));

            var role = new Role
            {
                RoleName = dto.RoleName.Trim(),
                RoleDescription = dto.RoleDescription ?? string.Empty
            };

            await _roleRepository.AddRoleAsync(role);
            return Ok(new ResponseResultSuccess<object>("Thêm vai trò thành công", MapToResponse(role)));
        }

        // 4. Cập nhật vai trò
        [Authorize(Policy = "role.assign")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleRequestDto dto)
        {
            var existing = await _roleRepository.GetRoleByIdAsync(id);
            if (existing == null)
                return NotFound(new ResponseResultError<object>($"Không tìm thấy vai trò với ID {id} để cập nhật!"));

            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new ResponseResultError<object>("Tên vai trò không được để trống!"));

            existing.RoleName = dto.RoleName.Trim();
            existing.RoleDescription = dto.RoleDescription ?? string.Empty;

            await _roleRepository.UpdateRoleAsync(existing);
            return Ok(new ResponseResultSuccess<object>("Cập nhật vai trò thành công", MapToResponse(existing)));
        }

        // 5. Xóa vai trò
        [Authorize(Policy = "role.assign")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new ResponseResultError<object>($"Không tìm thấy vai trò với ID {id} để xóa!"));

            await _roleRepository.DeleteRoleAsync(id);
            return Ok(new ResponseResultSuccess<object>("Xóa vai trò thành công"));
        }

        // 6. Gán quyền cho vai trò (thay thế toàn bộ)
        [Authorize(Policy = "role.assign")]
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] List<int> permissionIds)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new ResponseResultError<object>($"Không tìm thấy vai trò ID {id}!"));

            if (permissionIds == null)
                return BadRequest(new ResponseResultError<object>("Danh sách quyền không hợp lệ!"));

            await _roleRepository.AssignPermissionsAsync(id, permissionIds);

            // Đọc lại sau khi gán để trả về danh sách quyền mới
            var updated = await _roleRepository.GetRoleByIdAsync(id);
            return Ok(new ResponseResultSuccess<object>("Gán quyền thành công", MapToResponse(updated!)));
        }

        // --- MAPPER ---
        private static RoleResponseDto MapToResponse(Role r) => new()
        {
            Id = r.Id,
            RoleName = r.RoleName,
            RoleDescription = r.RoleDescription,
            Permissions = r.PermissionRoles?
                .Select(pr => pr.Permission?.Code ?? string.Empty)
                .Where(code => code.Length > 0)
                .Distinct()
                .ToList() ?? new List<string>()
        };
    }
}