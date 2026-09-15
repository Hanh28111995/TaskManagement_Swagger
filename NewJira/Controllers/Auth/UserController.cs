using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NewJira.Application.DTOs.Common;
using NewJira.Application.Interfaces.Repositories;

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
        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return Ok(new ResponseResultSuccess<object>(
                "Lấy danh sách người dùng thành công (Admin View)",
                users));
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
            if (user == null)
            {
                return NotFound(new ResponseResultError<object>(
                    "Không tìm thấy người dùng!"));
            }

            // Sửa ClaimTypes.Roles thành ClaimTypes.Role chuẩn của .NET
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(new ResponseResultSuccess<object>(
                "Lấy thông tin người dùng thành công",
                user));
        }

        // 4. Xóa người dùng (CHỈ ADMIN MỚI ĐƯỢC PHÉP THỰC HIỆN)
        [Authorize(Roles = "Admin")]
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
    }
}