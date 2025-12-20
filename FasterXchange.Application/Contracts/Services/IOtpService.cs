using FasterXchange.Application.Contracts.DTOs.Auth;
using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Services;

public interface IOtpService
{
    Task<RegisterResponseDto> SendRegistrationOtpAsync(string identifier, string? fullName = null);
    Task<LoginResponseDto> SendLoginOtpAsync(string identifier, string? deviceId = null);
    Task<bool> VerifyOtpAsync(string identifier, string code, OtpType type);
    Task<bool> IsOtpValidAsync(string identifier, string code, OtpType type);
    Task InvalidateOtpAsync(string identifier, OtpType type);
}

