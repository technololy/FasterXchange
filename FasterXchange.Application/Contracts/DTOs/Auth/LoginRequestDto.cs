namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class LoginRequestDto
{
    public string Identifier { get; set; } = string.Empty; // Email or phone
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

