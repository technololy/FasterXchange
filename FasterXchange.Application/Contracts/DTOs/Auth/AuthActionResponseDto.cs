namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class AuthActionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool? IsPinEnabled { get; set; }
    public bool? IsBiometricEnabled { get; set; }
}
