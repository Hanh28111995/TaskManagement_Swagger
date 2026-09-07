using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Application.Interfaces.Services;
using NewJira.Domain.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace NewJira.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IEmailService emailService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return null;

        // Lưu ý: Trong thực tế nên dùng PasswordHasher để kiểm tra, ở đây check theo dữ liệu thô/hash tùy DB hiện tại của bạn
        if (user.Password != password && user.PasswordHash != password)
        {
            return null;
        }

        return user;
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"] ?? "SuperSecretKeyWithAtLeast32BytesLength!";
        // Sửa thành:
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));

        var claims = new[]
        {
            new Claim("Id", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("Name", user.Name ?? string.Empty),
            new Claim("role", user.Roles ?? string.Empty) 
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<FirebaseTokenClaims?> VerifyFirebaseTokenAsync(string idToken)
    {
        try
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            // Lấy thông tin số điện thoại hoặc email từ Firebase claims nếu có
            string? phoneNumber = decodedToken.Claims.TryGetValue("phone_number", out var phone) ? phone.ToString() : null;

            return new FirebaseTokenClaims
            {
                Uid = decodedToken.Uid,
                PhoneNumber = phoneNumber,
                Email = decodedToken.Claims.TryGetValue("email", out var email) ? email.ToString() : null
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<User?> AuthenticateWithPhoneAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
        return await _userRepository.GetUserByPhoneNumberAsync(phoneNumber);
    }

    public async Task<User?> RegisterAsync(string email, string password, string name, string phoneNumber, string role)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(email);
        if (existingUser != null) return null; // Email đã tồn tại

        var newUser = new User
        {
            Email = email,
            Password = password, // Hoặc băm mật khẩu tại đây
            PasswordHash = "hashed_password",
            Name = name,
            PhoneNumber = phoneNumber,
            Roles = role
        };

        await _userRepository.AddUserAsync(newUser);
        await _userRepository.SaveUserChangesAsync();

        // Gửi email thông báo / xác thực sau khi tạo thành công
        await _emailService.SendEmailAsync(email, "Chào mừng đến với NewJira", $"Tài khoản của bạn đã được tạo thành công với vai trò: {role}.");

        return newUser;
    }
}