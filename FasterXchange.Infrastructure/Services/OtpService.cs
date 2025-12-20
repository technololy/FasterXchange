using FasterXchange.Application.Contracts.DTOs.Auth;
using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;
using OtpType = FasterXchange.Domain.Entities.OtpType;

namespace FasterXchange.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpiryMinutes = 10;
    private const int OtpLength = 6;
    private const int MaxAttempts = 3;

    public OtpService(
        IOtpRepository otpRepository,
        IUserRepository userRepository,
        ISmsService smsService,
        IEmailService emailService,
        ILogger<OtpService> logger)
    {
        _otpRepository = otpRepository;
        _userRepository = userRepository;
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> SendRegistrationOtpAsync(string identifier, string? fullName = null)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByIdentifierAsync(identifier);
        if (existingUser != null)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = "User already exists. Please login instead."
            };
        }

        // Invalidate any pending OTPs
        await _otpRepository.InvalidatePendingOtpsAsync(identifier, OtpType.Registration);

        // Generate OTP
        var otpCode = GenerateOtp();
        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            Identifier = identifier,
            Code = otpCode,
            Type = OtpType.Registration,
            Status = OtpStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes)
        };

        await _otpRepository.CreateAsync(otp);

        // Send OTP
        var isEmail = identifier.Contains("@");
        var deliveryMethod = isEmail ? OtpDeliveryMethod.Email : OtpDeliveryMethod.Sms;
        var maskedIdentifier = MaskIdentifier(identifier);

        try
        {
            if (isEmail)
            {
                await _emailService.SendOtpEmailAsync(identifier, otpCode, fullName);
            }
            else
            {
                await _smsService.SendOtpSmsAsync(identifier, otpCode);
            }

            return new RegisterResponseDto
            {
                Success = true,
                Message = "OTP sent successfully",
                OtpSentTo = maskedIdentifier,
                DeliveryMethod = deliveryMethod
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {Identifier}", identifier);
            return new RegisterResponseDto
            {
                Success = false,
                Message = "Failed to send OTP. Please try again."
            };
        }
    }

    public async Task<LoginResponseDto> SendLoginOtpAsync(string identifier, string? deviceId = null)
    {
        var user = await _userRepository.GetByIdentifierAsync(identifier);
        if (user == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "User not found. Please register first."
            };
        }

        // Check if account is locked
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Account is locked. Please try again in {remainingMinutes} minutes."
            };
        }

        // Invalidate any pending OTPs
        await _otpRepository.InvalidatePendingOtpsAsync(identifier, OtpType.Login);

        // Generate OTP
        var otpCode = GenerateOtp();
        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Identifier = identifier,
            Code = otpCode,
            Type = OtpType.Login,
            Status = OtpStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes)
        };

        await _otpRepository.CreateAsync(otp);

        // Check device trust
        var requiresDeviceVerification = false;
        if (!string.IsNullOrEmpty(deviceId))
        {
            // This will be checked in AuthService
            requiresDeviceVerification = true;
        }

        // Send OTP
        var isEmail = identifier.Contains("@");
        var deliveryMethod = isEmail ? OtpDeliveryMethod.Email : OtpDeliveryMethod.Sms;
        var maskedIdentifier = MaskIdentifier(identifier);

        try
        {
            if (isEmail)
            {
                await _emailService.SendOtpEmailAsync(identifier, otpCode, user.FullName);
            }
            else
            {
                await _smsService.SendOtpSmsAsync(identifier, otpCode);
            }

            return new LoginResponseDto
            {
                Success = true,
                Message = "OTP sent successfully",
                OtpSentTo = maskedIdentifier,
                DeliveryMethod = deliveryMethod,
                RequiresDeviceVerification = requiresDeviceVerification
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send login OTP to {Identifier}", identifier);
            return new LoginResponseDto
            {
                Success = false,
                Message = "Failed to send OTP. Please try again."
            };
        }
    }

    public async Task<bool> VerifyOtpAsync(string identifier, string code, OtpType type)
    {
        var otp = await _otpRepository.GetLatestOtpAsync(identifier, MapOtpType(type));
        if (otp == null)
        {
            return false;
        }

        // Check expiry
        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            otp.Status = OtpStatus.Expired;
            await _otpRepository.UpdateAsync(otp);
            return false;
        }

        // Check attempts
        if (otp.Attempts >= MaxAttempts)
        {
            otp.Status = OtpStatus.Failed;
            await _otpRepository.UpdateAsync(otp);
            return false;
        }

        // Verify code
        if (otp.Code != code)
        {
            otp.Attempts++;
            if (otp.Attempts >= MaxAttempts)
            {
                otp.Status = OtpStatus.Failed;
            }
            await _otpRepository.UpdateAsync(otp);
            return false;
        }

        // Mark as verified
        otp.Status = OtpStatus.Verified;
        otp.VerifiedAt = DateTime.UtcNow;
        await _otpRepository.UpdateAsync(otp);

        return true;
    }

    public async Task<bool> IsOtpValidAsync(string identifier, string code, OtpType type)
    {
        return await VerifyOtpAsync(identifier, code, type);
    }

    public async Task InvalidateOtpAsync(string identifier, OtpType type)
    {
        await _otpRepository.InvalidatePendingOtpsAsync(identifier, MapOtpType(type));
    }

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string MaskIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return string.Empty;

        if (identifier.Contains("@"))
        {
            // Email masking: a***@example.com
            var parts = identifier.Split('@');
            if (parts[0].Length > 1)
            {
                return $"{parts[0][0]}***@{parts[1]}";
            }
            return $"***@{parts[1]}";
        }
        else
        {
            // Phone masking: +1***1234
            if (identifier.Length > 4)
            {
                return $"{identifier.Substring(0, 2)}***{identifier.Substring(identifier.Length - 4)}";
            }
            return "***";
        }
    }

    private OtpType MapOtpType(OtpType type)
    {
        return type; // Already the same type
    }
}

