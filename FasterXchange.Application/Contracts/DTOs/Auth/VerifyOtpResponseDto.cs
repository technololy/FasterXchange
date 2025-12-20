namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class VerifyOtpResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
    public bool RequiresKyc { get; set; }
}

