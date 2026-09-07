namespace NewJira.Application.DTOs.Auth;

public class LoginResponseDto
{
  public int Id { get; set; }

  public string Email { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public string Roles { get; set; } = string.Empty;

  public string? Avatar { get; set; }

  public string? PhoneNumber { get; set; }

  public string AccessToken { get; set; } = string.Empty;
}


