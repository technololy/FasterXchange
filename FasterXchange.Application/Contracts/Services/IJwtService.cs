using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
    Task SaveRefreshTokenAsync(string refreshToken, Guid userId, DateTime expiresAt);
}

