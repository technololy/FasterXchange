using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly ApplicationDbContext _context;

    public OtpRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OtpCode?> GetLatestOtpAsync(string identifier, OtpType type)
    {
        return await _context.OtpCodes
            .Where(o => o.Identifier == identifier && o.Type == type && o.Status == OtpStatus.Pending)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<OtpCode> CreateAsync(OtpCode otpCode)
    {
        _context.OtpCodes.Add(otpCode);
        await _context.SaveChangesAsync();
        return otpCode;
    }

    public async Task<OtpCode> UpdateAsync(OtpCode otpCode)
    {
        _context.OtpCodes.Update(otpCode);
        await _context.SaveChangesAsync();
        return otpCode;
    }

    public async Task InvalidatePendingOtpsAsync(string identifier, OtpType type)
    {
        var pendingOtps = await _context.OtpCodes
            .Where(o => o.Identifier == identifier && o.Type == type && o.Status == OtpStatus.Pending)
            .ToListAsync();

        foreach (var otp in pendingOtps)
        {
            otp.Status = OtpStatus.Expired;
        }

        await _context.SaveChangesAsync();
    }
}

