namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class BiometricTrustRequestDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public string? BiometricKey { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
