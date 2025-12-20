namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? OtpSentTo { get; set; } // Masked email or phone
    public OtpDeliveryMethod? DeliveryMethod { get; set; }
}

public enum OtpDeliveryMethod
{
    Email,
    Sms
}

