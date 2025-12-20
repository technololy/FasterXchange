using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByDeviceIdAsync(Guid userId, string deviceId)
    {
        return await _context.Devices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId);
    }

    public async Task<Device> CreateAsync(Device device)
    {
        device.FirstSeenAt = DateTime.UtcNow;
        device.LastSeenAt = DateTime.UtcNow;
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<Device> UpdateAsync(Device device)
    {
        device.LastSeenAt = DateTime.UtcNow;
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<List<Device>> GetUserDevicesAsync(Guid userId)
    {
        return await _context.Devices
            .Where(d => d.UserId == userId && d.IsActive)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync();
    }
}

