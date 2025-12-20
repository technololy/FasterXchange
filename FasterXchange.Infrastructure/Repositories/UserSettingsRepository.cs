using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public UserSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<UserSettings> CreateAsync(UserSettings settings)
    {
        settings.CreatedAt = DateTime.UtcNow;
        _context.UserSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateAsync(UserSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _context.UserSettings.Update(settings);
        await _context.SaveChangesAsync();
        return settings;
    }
}

