namespace FasterXchange.Infrastructure.ExternalServices;

public interface ISmsService
{
    Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode);
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}

