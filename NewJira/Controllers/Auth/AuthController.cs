using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Hosting;
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
        private readonly ITokenService _tokenService;

        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, ITokenService tokenService, IWebHostEnvironment env)
        {
            _authService = authService;
            _tokenService = tokenService;
            _env = env;
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

            var tokens = await _tokenService.IssueTokensAsync(user);

            var responseDto = new LoginResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Roles = user.Role?.RoleName ?? "Member",
                Avatar = user.Avatar,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                AccessToken = tokens.accessToken,
            };
            SetRefreshTokenCookie(tokens.refreshToken);   
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
            var firebaseClaims = await _authService.VerifyFirebaseTokenAsync(model.IdToken);
            if (firebaseClaims == null || string.IsNullOrWhiteSpace(firebaseClaims.PhoneNumber))
            {
                return BadRequest(new ResponseResultError<object>(
                    "Mã xác thực số điện thoại không hợp lệ hoặc đã hết hạn!"));
            }            
            var user = await _authService.AuthenticateWithPhoneAsync(firebaseClaims.PhoneNumber);
            
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
            var tokens = await _tokenService.IssueTokensAsync(user);
            var responseDto = new LoginResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Roles = user.Role?.RoleName ?? "Member",
                Avatar = user.Avatar,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                AccessToken = tokens.accessToken
            };
            SetRefreshTokenCookie(tokens.refreshToken);

            return Ok(new ResponseResultSuccess<object>(
                "Xác thực OTP thành công",
                responseDto));
        }

        [Authorize(Policy = "user.manage")]
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

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest(new ResponseResultError<object>("Thiếu refresh token!"));

            var tokens = await _tokenService.RefreshAsync(refreshToken);
            if (tokens == null)
                return Unauthorized(new ResponseResultError<object>("Phiên đăng nhập hết hạn!"));

            SetRefreshTokenCookie(tokens.Value.refreshToken);   // xoay vòng cookie
            var newToken = new RefreshTokenRequestDto
            {
                RefreshToken = tokens.Value.accessToken
            };
            return Ok(new ResponseResultSuccess<object>("Cấp token mới thành công", newToken));
        }


        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await _tokenService.RevokeAsync(refreshToken);

            Response.Cookies.Delete("refreshToken");
            return Ok(new ResponseResultSuccess<object>("Đã đăng xuất", null));
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,        
                Secure = true,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
    }
}