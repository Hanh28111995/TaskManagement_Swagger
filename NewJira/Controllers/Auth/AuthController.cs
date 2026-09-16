using Microsoft.AspNetCore.Authorization; // Cần thiết cho [Authorize]
using Microsoft.AspNetCore.Mvc;
using NewJira.Application.DTOs.Auth;
using NewJira.Application.DTOs.Common;
using NewJira.Application.Interfaces.Services;
using Newtonsoft.Json.Linq;

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
                Roles = user.Role?.RoleName ?? "Member",
                Avatar = user.Avatar,                
                AccessToken = token
            };

            return Ok(new ResponseResultSuccess<object>(
                "Đăng nhập truyền thống thành công",
                responseDto));
        }
                
        [HttpPost("check-phone")]
        public async Task<IActionResult> CheckPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(new ResponseResultError<object>("Vui lòng nhập số điện thoại!"));

            var user = await _authService.AuthenticateWithPhoneAsync(phone);

            return Ok(new ResponseResultSuccess<object>(
                user == null ? "Số chưa đăng ký" : "Số đã đăng ký",
                new { isRegistered = user != null }));
        }

        [HttpPost("validate-phone-code")]        
            public async Task<IActionResult> ValidatePhoneCode([FromBody] FirebaseLoginDto model)
            {
                if (model == null || string.IsNullOrWhiteSpace(model.IdToken))
                {
                    return BadRequest(new ResponseResultError<object>(
                        "Thiếu idToken!"));
                }

                // 1. Verify token Firebase — nếu OTP sai/hết hạn -> null
                var firebaseClaims = await _authService.VerifyFirebaseTokenAsync(model.IdToken);
                if (firebaseClaims == null || string.IsNullOrWhiteSpace(firebaseClaims.PhoneNumber))
                {
                    return BadRequest(new ResponseResultError<object>(
                        "Mã xác thực số điện thoại không hợp lệ hoặc đã hết hạn!"));
                }

                // 2. Tra xem số này đã đăng ký trong hệ thống chưa
                var user = await _authService.AuthenticateWithPhoneAsync(firebaseClaims.PhoneNumber);

                // 3a. CHƯA đăng ký -> KHÔNG phải lỗi, trả isRegistered=false
                //     để FE chuyển sang form đăng ký
                if (user == null)
                {
                    return Ok(new ResponseResultSuccess<object>(
                        "Số điện thoại hợp lệ nhưng chưa đăng ký tài khoản",
                        new
                        {
                            isRegistered = false,
                            phoneNumber = firebaseClaims.PhoneNumber,
                            firebaseUid = firebaseClaims.Uid
                        }));
                }
            var token = _authService.GenerateJwtToken(user);
            var responseDto = new LoginResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Roles = user.Role?.RoleName ?? "Member",
                Avatar = user.Avatar,
                AccessToken = token
            };
            // 3b. ĐÃ đăng ký -> trả cờ true + thông tin tối thiểu

            return Ok(new ResponseResultSuccess<object>(
                "Xác thực OTP thành công",
                responseDto));            
            }           

    [Authorize(Roles = "Admin")] // Khóa bảo mật: Phải có Token mang quyền Admin mới gọi được regisrter
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