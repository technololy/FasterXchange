using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAndCurrencyAsync(Guid userId, Currency currency);
    Task<Wallet> CreateAsync(Wallet wallet);
    Task<Wallet> UpdateAsync(Wallet wallet);
    Task<List<Wallet>> GetUserWalletsAsync(Guid userId);
    Task<Wallet?> GetByIdAsync(Guid walletId);
}
