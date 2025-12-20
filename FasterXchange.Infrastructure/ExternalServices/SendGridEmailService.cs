using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FasterXchange.Infrastructure.ExternalServices;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string? _apiKey;
    private readonly string? _fromEmail;
    private readonly string? _fromName;

    public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["SendGrid:ApiKey"];
        _fromEmail = _configuration["SendGrid:FromEmail"];
        _fromName = _configuration["SendGrid:FromName"] ?? "FasterXchange";
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode, string? recipientName = null)
    {
        var subject = "Your FasterXchange Verification Code";
        var body = $@"
            <html>
            <body>
                <h2>Hello {(string.IsNullOrEmpty(recipientName) ? "there" : recipientName)},</h2>
                <p>Your verification code is: <strong>{otpCode}</strong></p>
                <p>This code is valid for 10 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>FasterXchange Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendEmailAsync(string email, string subject, string body)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_fromEmail))
        {
            _logger.LogWarning("SendGrid credentials not configured. Email will not be sent.");
            // In development, log the email instead
            _logger.LogInformation("Email to {Email} - Subject: {Subject}\nBody: {Body}", email, subject, body);
            return true; // Return true for development
        }

        try
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", email);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Body: {Body}", 
                    email, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            return false;
        }
    }
}

