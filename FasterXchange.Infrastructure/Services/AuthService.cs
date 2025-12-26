using FasterXchange.Application.Contracts.DTOs;
using FasterXchange.Application.Contracts.DTOs.Auth;
using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.Extensions.Logging;
using OtpType = FasterXchange.Domain.Entities.OtpType;
using BCryptNet = BCrypt.Net.BCrypt;

namespace FasterXchange.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly INotificationPreferencesRepository _notificationPreferencesRepository;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private const int MaxLoginFailures = 5;
    private const int LockoutMinutes = 15;
    private const int MaxPinFailures = 5;

    public AuthService(
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        IWalletRepository walletRepository,
        IUserSettingsRepository userSettingsRepository,
        INotificationPreferencesRepository notificationPreferencesRepository,
        IOtpService otpService,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _walletRepository = walletRepository;
        _userSettingsRepository = userSettingsRepository;
        _notificationPreferencesRepository = notificationPreferencesRepository;
        _otpService = otpService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = "Email or phone number is required"
            };
        }

        var identifier = request.Email ?? request.PhoneNumber!;

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

        // Send OTP
        return await _otpService.SendRegistrationOtpAsync(identifier, request.FullName);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Email or phone number is required"
            };
        }

        // Send OTP
        return await _otpService.SendLoginOtpAsync(request.Identifier, request.DeviceId);
    }

    public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Code))
        {
            return new VerifyOtpResponseDto
            {
                Success = false,
                Message = "Identifier and OTP code are required"
            };
        }

        // Verify OTP
        var isValid = await _otpService.VerifyOtpAsync(
            request.Identifier,
            request.Code,
            request.Type);

        if (!isValid)
        {
            await HandleFailedLoginAsync(request);
            return new VerifyOtpResponseDto
            {
                Success = false,
                Message = "Invalid or expired OTP code"
            };
        }

        // Get or create user based on OTP type
        User? user = null;

        if (request.Type == OtpType.Registration)
        {
            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Identifier.Contains("@") ? request.Identifier : null,
                PhoneNumber = !request.Identifier.Contains("@") ? request.Identifier : null,
                Status = UserStatus.Unverified,
                KycStatus = KycStatus.NotStarted,
                CreatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);

            // Create default wallets
            var cadWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Currency = Currency.CAD,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            var ngnWallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Currency = Currency.NGN,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _walletRepository.CreateAsync(cadWallet);
            await _walletRepository.CreateAsync(ngnWallet);

            // Create default settings
            var settings = new UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _userSettingsRepository.CreateAsync(settings);

            // Create default notification preferences
            var notificationPreferences = new NotificationPreferences
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationPreferencesRepository.CreateAsync(notificationPreferences);
        }
        else if (request.Type == OtpType.Login)
        {
            // Get existing user
            user = await _userRepository.GetByIdentifierAsync(request.Identifier);
            if (user == null)
            {
                return new VerifyOtpResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if account is locked
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                var remainingMinutes = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                return new VerifyOtpResponseDto
                {
                    Success = false,
                    Message = $"Account is locked. Please try again in {remainingMinutes} minutes."
                };
            }

            // Reset failed login attempts
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        if (user == null)
        {
            return new VerifyOtpResponseDto
            {
                Success = false,
                Message = "Unable to process verification"
            };
        }

        // Register device if provided
        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            var existingDevice = await _deviceRepository.GetByDeviceIdAsync(user.Id, request.DeviceId);
            if (existingDevice == null)
            {
                var device = new Device
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DeviceId = request.DeviceId,
                    DeviceName = request.DeviceName,
                    DeviceType = request.DeviceType,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    IsTrusted = false, // First time device
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };

                await _deviceRepository.CreateAsync(device);
            }
            else
            {
                existingDevice.LastSeenAt = DateTime.UtcNow;
                existingDevice.IpAddress = request.IpAddress;
                existingDevice.UserAgent = request.UserAgent;
                await _deviceRepository.UpdateAsync(existingDevice);
            }
        }

        // Generate JWT tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60); // Default 60 minutes

        await _jwtService.SaveRefreshTokenAsync(refreshToken, user.Id, expiresAt.AddDays(30));

        // Map user to DTO
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            CountryOfResidence = user.CountryOfResidence,
            PreferredCurrency = user.PreferredCurrency,
            Language = user.Language,
            IsTransactionPinEnabled = user.IsTransactionPinEnabled,
            IsBiometricEnabled = user.IsBiometricEnabled,
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            IsBalanceVisible = user.IsBalanceVisible,
            Status = user.Status.ToString(),
            KycStatus = user.KycStatus.ToString(),
            CreatedAt = user.CreatedAt
        };

        return new VerifyOtpResponseDto
        {
            Success = true,
            Message = "OTP verified successfully",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = userDto,
            RequiresKyc = user.KycStatus != KycStatus.Approved
        };
    }

    public async Task<AuthActionResponseDto> SetTransactionPinAsync(Guid userId, SetPinRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Pin) || string.IsNullOrWhiteSpace(request.ConfirmPin))
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "PIN and confirmation are required",
                IsPinEnabled = false
            };
        }

        if (request.Pin != request.ConfirmPin)
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "PINs do not match",
                IsPinEnabled = false
            };
        }

        if (request.Pin.Length is < 4 or > 6 || !request.Pin.All(char.IsDigit))
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "PIN must be 4-6 digits",
                IsPinEnabled = false
            };
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "User not found",
                IsPinEnabled = false
            };
        }

        user.TransactionPinHash = BCryptNet.HashPassword(request.Pin);
        user.IsTransactionPinEnabled = true;
        user.FailedPinAttempts = 0;
        user.PinLockedUntil = null;
        await _userRepository.UpdateAsync(user);

        return new AuthActionResponseDto
        {
            Success = true,
            Message = "Transaction PIN set successfully",
            IsPinEnabled = true
        };
    }

    public async Task<AuthActionResponseDto> VerifyTransactionPinAsync(Guid userId, VerifyPinRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "User not found",
                IsPinEnabled = false
            };
        }

        if (!user.IsTransactionPinEnabled || string.IsNullOrEmpty(user.TransactionPinHash))
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "Transaction PIN not set",
                IsPinEnabled = false
            };
        }

        if (user.PinLockedUntil.HasValue && user.PinLockedUntil > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.PinLockedUntil.Value - DateTime.UtcNow).TotalMinutes;
            return new AuthActionResponseDto
            {
                Success = false,
                Message = $"PIN is locked. Try again in {remainingMinutes} minutes.",
                IsPinEnabled = true
            };
        }

        var isValid = BCryptNet.Verify(request.Pin, user.TransactionPinHash);
        if (!isValid)
        {
            user.FailedPinAttempts++;
            if (user.FailedPinAttempts >= MaxPinFailures)
            {
                user.PinLockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            }
            await _userRepository.UpdateAsync(user);

            return new AuthActionResponseDto
            {
                Success = false,
                Message = "Invalid PIN",
                IsPinEnabled = true
            };
        }

        user.FailedPinAttempts = 0;
        user.PinLockedUntil = null;
        await _userRepository.UpdateAsync(user);

        return new AuthActionResponseDto
        {
            Success = true,
            Message = "PIN verified",
            IsPinEnabled = true
        };
    }

    public async Task<TrustedDeviceDto?> TrustDeviceForBiometricsAsync(Guid userId, BiometricTrustRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
            return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var device = await _deviceRepository.GetByDeviceIdAsync(userId, request.DeviceId);
        if (device == null)
        {
            device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                IsTrusted = true,
                BiometricKey = request.BiometricKey,
                TrustedAt = DateTime.UtcNow,
                FirstSeenAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            };
            device = await _deviceRepository.CreateAsync(device);
        }
        else
        {
            device.IsTrusted = true;
            device.BiometricKey = request.BiometricKey ?? device.BiometricKey;
            device.TrustedAt = DateTime.UtcNow;
            device.DeviceName = request.DeviceName ?? device.DeviceName;
            device.DeviceType = request.DeviceType ?? device.DeviceType;
            device.IpAddress = request.IpAddress ?? device.IpAddress;
            device.UserAgent = request.UserAgent ?? device.UserAgent;
            device.LastSeenAt = DateTime.UtcNow;
            device = await _deviceRepository.UpdateAsync(device);
        }

        user.IsBiometricEnabled = true;
        await _userRepository.UpdateAsync(user);

        return MapToTrustedDeviceDto(device);
    }

    public async Task<AuthActionResponseDto> DisableBiometricsForDeviceAsync(Guid userId, string deviceId)
    {
        var device = await _deviceRepository.GetByDeviceIdAsync(userId, deviceId);
        if (device == null)
        {
            return new AuthActionResponseDto
            {
                Success = false,
                Message = "Device not found"
            };
        }

        device.IsTrusted = false;
        device.BiometricKey = null;
        device.TrustedAt = null;
        await _deviceRepository.UpdateAsync(device);

        var devices = await _deviceRepository.GetUserDevicesAsync(userId);
        var stillTrusted = devices.Any(d => d.IsTrusted);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.IsBiometricEnabled = stillTrusted;
            await _userRepository.UpdateAsync(user);
        }

        return new AuthActionResponseDto
        {
            Success = true,
            Message = "Biometrics disabled for device",
            IsBiometricEnabled = stillTrusted
        };
    }

    public async Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(Guid userId)
    {
        var devices = await _deviceRepository.GetUserDevicesAsync(userId);
        return devices
            .Where(d => d.IsActive)
            .Select(MapToTrustedDeviceDto)
            .ToList();
    }

    public async Task<bool> CheckDeviceTrustAsync(Guid userId, string deviceId)
    {
        var device = await _deviceRepository.GetByDeviceIdAsync(userId, deviceId);
        return device != null && device.IsTrusted;
    }

    public async Task<Device> RegisterDeviceAsync(
        Guid userId,
        string deviceId,
        string? deviceName,
        string? deviceType,
        string? ipAddress,
        string? userAgent)
    {
        var existingDevice = await _deviceRepository.GetByDeviceIdAsync(userId, deviceId);
        if (existingDevice != null)
        {
            existingDevice.LastSeenAt = DateTime.UtcNow;
            existingDevice.IpAddress = ipAddress;
            existingDevice.UserAgent = userAgent;
            return await _deviceRepository.UpdateAsync(existingDevice);
        }

        var device = new Device
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            DeviceType = deviceType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsTrusted = false,
            FirstSeenAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        return await _deviceRepository.CreateAsync(device);
    }

    private async Task HandleFailedLoginAsync(VerifyOtpRequestDto request)
    {
        if (request.Type != OtpType.Login)
            return;

        var user = await _userRepository.GetByIdentifierAsync(request.Identifier);
        if (user == null)
            return;

        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= MaxLoginFailures)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
        }
        await _userRepository.UpdateAsync(user);
    }

    private TrustedDeviceDto MapToTrustedDeviceDto(Device device)
    {
        return new TrustedDeviceDto
        {
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            IsTrusted = device.IsTrusted,
            TrustedAt = device.TrustedAt,
            FirstSeenAt = device.FirstSeenAt,
            LastSeenAt = device.LastSeenAt
        };
    }
}
