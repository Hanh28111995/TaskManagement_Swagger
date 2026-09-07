#nullable enable
namespace NewJira.Application.Interfaces.Services;

public class FirebaseTokenClaims
{
    public string Uid { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}