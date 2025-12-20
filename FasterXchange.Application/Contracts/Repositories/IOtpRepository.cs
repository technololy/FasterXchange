using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface IOtpRepository
{
    Task<OtpCode?> GetLatestOtpAsync(string identifier, OtpType type);
    Task<OtpCode> CreateAsync(OtpCode otpCode);
    Task<OtpCode> UpdateAsync(OtpCode otpCode);
    Task InvalidatePendingOtpsAsync(string identifier, OtpType type);
}

