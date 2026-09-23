using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.DTOs.Common;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Domain.Entities;
using System.Security.Claims;

namespace NewJira.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // 1. Lấy danh sách người dùng đầy đủ (Chỉ Admin)
        [Authorize(Policy = "user.manage")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();           
            var data = users.Select(MapToResponse);
            return Ok(new ResponseResultSuccess<object>("Lấy danh sách người dùng thành công (Admin View)", data));
        }

        // 2. Lấy danh sách thu gọn cho Member List (Yêu cầu đăng nhập nói chung)
        [Authorize]
        [HttpGet("get-all-users-for-memberlist")]
        public async Task<IActionResult> GetAllUsersForMemberList()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var restrictedUsers = users.Select(user => new
            {
                user.Id,
                user.Name,
                user.Role?.RoleName 
            });

            return Ok(new ResponseResultSuccess<object>(
                "Lấy danh sách người dùng thành công (Member View)",
                restrictedUsers));
        }

        // 3. Lấy chi tiết thông tin người dùng theo ID
        [Authorize]
        [HttpGet("get-user-detail/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            var data = MapToResponse(user);
            if (data == null)
            {
                return NotFound(new ResponseResultError<object>(
                    "Không tìm thấy người dùng!"));
            }                      

            return Ok(new ResponseResultSuccess<object>(
                "Lấy thông tin người dùng thành công",
                data));
        }

        [Authorize(Policy = "user.manage")]
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound(new ResponseResultError<object>("Không tìm thấy người dùng!"));

            user.Name = dto.Name ?? user.Name;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            if (dto.RoleId.HasValue) user.RoleId = dto.RoleId.Value;

            await _userRepository.UpdateUserAsync(user);
            var data = MapToResponse(user);
            return Ok(new ResponseResultSuccess<object>("Cập nhật người dùng thành công", data));
        }

        // 4. Xóa người dùng (CHỈ ADMIN MỚI ĐƯỢC PHÉP THỰC HIỆN)
        [Authorize(Policy = "user.manage")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new ResponseResultError<object>(
                    "Không tìm thấy người dùng để xóa!"));
            }

            await _userRepository.DeleteUserAsync(id);
            return Ok(new ResponseResultSuccess<object>(
                "Xóa người dùng thành công"));
        }

        private static UserResponseDto MapToResponse(User u) => new()
        {
            Id = u.Id,
            Email = u.Email ?? "",
            Name = u.Name ?? "",
            Roles = u.Role?.RoleName ?? "Member",
            Avatar = u.Avatar,
            PhoneNumber = u.PhoneNumber
        };
    }
}