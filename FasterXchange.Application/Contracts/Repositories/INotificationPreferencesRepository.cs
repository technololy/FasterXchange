using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByUserIdAsync(Guid userId);
    Task<NotificationPreferences> CreateAsync(NotificationPreferences preferences);
    Task<NotificationPreferences> UpdateAsync(NotificationPreferences preferences);
}

