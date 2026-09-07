using System.Threading.Tasks;
using NewJira.Domain.Entities;

#nullable enable
namespace NewJira.Application.Interfaces.Services;

public interface IAuthService
{
    // --- Phần xác thực truyền thống & Token ---
    Task<User?> LoginAsync(string email, string password);
    string GenerateJwtToken(User user);

    // --- Phần xác thực Firebase & Điện thoại ---
    Task<FirebaseTokenClaims?> VerifyFirebaseTokenAsync(string idToken);
    Task<User?> AuthenticateWithPhoneAsync(string phoneNumber);

    // --- Phần Đăng ký tài khoản ---
    Task<User?> RegisterAsync(string email, string password, string name, string phoneNumber, string role);
}