using FasterXchange.Application.Contracts.DTOs.Auth;
using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request);
    Task<bool> CheckDeviceTrustAsync(Guid userId, string deviceId);
    Task<Device> RegisterDeviceAsync(Guid userId, string deviceId, string? deviceName, string? deviceType, string? ipAddress, string? userAgent);
    Task<AuthActionResponseDto> SetTransactionPinAsync(Guid userId, SetPinRequestDto request);
    Task<AuthActionResponseDto> VerifyTransactionPinAsync(Guid userId, VerifyPinRequestDto request);
    Task<TrustedDeviceDto?> TrustDeviceForBiometricsAsync(Guid userId, BiometricTrustRequestDto request);
    Task<AuthActionResponseDto> DisableBiometricsForDeviceAsync(Guid userId, string deviceId);
    Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(Guid userId);
}
