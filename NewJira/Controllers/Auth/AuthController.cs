using Microsoft.AspNetCore.Authorization; // Cần thiết cho [Authorize]
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.DTOs.Common;
using NewJira.Application.Interfaces.Services;

namespace NewJira.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // 1. Đăng nhập truyền thống (Email/Password)
        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            var user = await _authService.LoginAsync(model.Email, model.Password);
            if (user == null)
            {
                return BadRequest(new ResponseResultError<object>(
                    "Email hoặc mật khẩu không chính xác!"));
            }

            var token = _authService.GenerateJwtToken(user);

            // Ánh xạ sang LoginResponseDto
            var responseDto = new LoginResponseDto
            {
                Id = user.Id,                
                Name = user.Name,
                Roles = user.Roles,
                Avatar = user.Avatar,                
                AccessToken = token
            };

            return Ok(new ResponseResultSuccess<object>(
                "Đăng nhập truyền thống thành công",
                responseDto));
        }

        // 2. Đăng nhập bằng Firebase Phone OTP
        [HttpPost("signin-firebase")]
        public async Task<IActionResult> LoginWithFirebase([FromBody] FirebaseLoginDto model)
        {
            var firebaseClaims = await _authService.VerifyFirebaseTokenAsync(model.IdToken);
            if (firebaseClaims == null)
            {
                return BadRequest(new ResponseResultError<object>(
                    "Xác thực Firebase Token thất bại hoặc không hợp lệ!"));
            }

            var user = await _authService.AuthenticateWithPhoneAsync(firebaseClaims.PhoneNumber);
            if (user == null)
            {
                return BadRequest(new ResponseResultError<object>(
                    "Số điện thoại này chưa được đăng ký trong hệ thống!"));
            }

            var jwtToken = _authService.GenerateJwtToken(user);

            // Ánh xạ sang LoginResponseDto
            var responseDto = new LoginResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Roles = user.Roles,
                Avatar = user.Avatar,                
                AccessToken = jwtToken
            };

            return Ok(new ResponseResultSuccess<object>(
                "Đăng nhập Firebase thành công",
                responseDto));
        }

        [HttpPost("validate-phone-code")]
        public async Task<IActionResult> ValidatePhoneCode([FromBody] FirebaseLoginDto model)
        {
            var firebaseClaims = await _authService.VerifyFirebaseTokenAsync(model.IdToken);
            if (firebaseClaims == null || string.IsNullOrWhiteSpace(firebaseClaims.PhoneNumber))
            {
                return BadRequest(new ResponseResultError<object>(
                    "Mã xác thực số điện thoại không hợp lệ hoặc đã hết hạn!"));
            }

            return Ok(new ResponseResultSuccess<object>(
                "Xác thực số điện thoại thành công",
                new
                {
                    phoneNumber = firebaseClaims.PhoneNumber,
                    firebaseUid = firebaseClaims.Uid
                }));
        }

        // 3. Đăng ký tài khoản mới (CHỈ ADMIN MỚI ĐƯỢC PHÉP TẠO) + Gửi mail xác thực Firebase
        [Authorize(Roles = "Admin")] // Khóa bảo mật: Phải có Token mang quyền Admin mới gọi được
        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(
                model.Email,
                model.Password,
                model.Name,
                model.PhoneNumber ?? string.Empty,
                model.Role ?? "Member"
            );

            if (result == null)
            {
                return BadRequest(new ResponseResultError<object>(
                    "Email đã tồn tại hoặc tạo tài khoản thất bại!"));
            }

            return Ok(new ResponseResultSuccess<object>(
                "Tạo tài khoản thành công! Đã gửi email xác thực.",
                result));
        }
    }
}