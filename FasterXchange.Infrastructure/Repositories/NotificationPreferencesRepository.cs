using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationPreferencesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferences?> GetByUserIdAsync(Guid userId)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(n => n.UserId == userId);
    }

    public async Task<NotificationPreferences> CreateAsync(NotificationPreferences preferences)
    {
        preferences.CreatedAt = DateTime.UtcNow;
        _context.NotificationPreferences.Add(preferences);
        await _context.SaveChangesAsync();
        return preferences;
    }

    public async Task<NotificationPreferences> UpdateAsync(NotificationPreferences preferences)
    {
        preferences.UpdatedAt = DateTime.UtcNow;
        _context.NotificationPreferences.Update(preferences);
        await _context.SaveChangesAsync();
        return preferences;
    }
}

