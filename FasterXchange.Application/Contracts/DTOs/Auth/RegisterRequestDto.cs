namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class RegisterRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
}

