using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(Guid userId);
    Task<UserSettings> CreateAsync(UserSettings settings);
    Task<UserSettings> UpdateAsync(UserSettings settings);
}

