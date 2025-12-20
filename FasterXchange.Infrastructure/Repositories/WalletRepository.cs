using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByUserIdAndCurrencyAsync(Guid userId, Currency currency)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Currency == currency);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        wallet.CreatedAt = DateTime.UtcNow;
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<List<Wallet>> GetUserWalletsAsync(Guid userId)
    {
        return await _context.Wallets
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    public async Task<Wallet> UpdateAsync(Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet?> GetByIdAsync(Guid walletId)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId);
    }
}

