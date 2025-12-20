namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? OtpSentTo { get; set; }
    public OtpDeliveryMethod? DeliveryMethod { get; set; }
    public bool RequiresDeviceVerification { get; set; }
}

