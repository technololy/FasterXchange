namespace FasterXchange.Infrastructure.ExternalServices;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string email, string otpCode, string? recipientName = null);
    Task<bool> SendEmailAsync(string email, string subject, string body);
}

