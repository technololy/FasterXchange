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
}

