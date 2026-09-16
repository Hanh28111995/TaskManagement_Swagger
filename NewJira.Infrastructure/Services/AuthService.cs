using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NewJira.Application.Interfaces.Repositories;
using NewJira.Application.Interfaces.Services;
using NewJira.Application.Helpers;
using NewJira.Domain.Entities;
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

        if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash != "hashed_password"
           && PasswordHelper.Verify(password, user.PasswordHash))
        {
            return user;
        }

        if (user.Password != null && user.Password == password)
        {
            user.PasswordHash = PasswordHelper.Hash(password);
            user.Password = null;
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveUserChangesAsync();
            return user;
        }
        
        return null;
    }

    public string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"]
                        ?? "SuperSecretKeyWithAtLeast32BytesLength!";
        return JwtHelper.GenerateToken(user, secretKey);
    }

    public async Task<FirebaseTokenClaims?> VerifyFirebaseTokenAsync(string idToken)
    {
        try
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            string? phoneNumber = decodedToken.Claims.TryGetValue("phone_number", out var phone)
                ? phone.ToString() : null;

            return new FirebaseTokenClaims
            {
                Uid = decodedToken.Uid,
                PhoneNumber = phoneNumber,
                Email = decodedToken.Claims.TryGetValue("email", out var email)
                    ? email.ToString() : null
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

        return await _userRepository.GetUserByPhoneNumberAsync( PhoneHelper.NormalizeToLocal(phoneNumber));
    }

    public async Task<User?> RegisterAsync(string email, string password, string name, string phoneNumber, string role)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(email);
        if (existingUser != null) return null; 

        var newUser = new User
        {
            Email = email,
            Password = password, 
            PasswordHash = PasswordHelper.Hash(password),
            Name = name,
            PhoneNumber = PhoneHelper.NormalizeToLocal(phoneNumber),
            RoleId = ResolveRoleId(role),
        };

        await _userRepository.AddUserAsync(newUser);
        await _userRepository.SaveUserChangesAsync();
        
        await _emailService.SendEmailAsync(email, "Chào mừng đến với NewJira", $"Tài khoản của bạn đã được tạo thành công với vai trò: {role}.");

        return newUser;
    }

    // Thêm vào AuthService: resolve RoleId
    private static int ResolveRoleId(string role) => role?.ToLower() switch
    {
        "admin" => 1,
        "manager" => 2,
        _ => 3, // Member
    };
}