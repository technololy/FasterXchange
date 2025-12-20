using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetByDeviceIdAsync(Guid userId, string deviceId);
    Task<Device> CreateAsync(Device device);
    Task<Device> UpdateAsync(Device device);
    Task<List<Device>> GetUserDevicesAsync(Guid userId);
}

