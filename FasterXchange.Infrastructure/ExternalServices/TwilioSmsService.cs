using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace FasterXchange.Infrastructure.ExternalServices;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromPhoneNumber;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _accountSid = _configuration["Twilio:AccountSid"];
        _authToken = _configuration["Twilio:AuthToken"];
        _fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"];

        if (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken))
        {
            TwilioClient.Init(_accountSid, _authToken);
        }
    }

    public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode)
    {
        var message = $"Your FasterXchange verification code is: {otpCode}. Valid for 10 minutes.";
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_fromPhoneNumber))
        {
            _logger.LogWarning("Twilio credentials not configured. SMS will not be sent.");
            // In development, log the OTP instead
            _logger.LogInformation("SMS OTP for {PhoneNumber}: {Otp}", phoneNumber, message);
            return true; // Return true for development
        }

        try
        {
            var twilioMessage = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {Sid}", phoneNumber, twilioMessage.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}

