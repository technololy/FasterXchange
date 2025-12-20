using FasterXchange.Application.Contracts.DTOs.Wallet;
using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Services;

public interface IWalletService
{
    Task<List<WalletBalanceDto>> GetUserWalletsAsync(Guid userId);
    Task<WalletBalanceDto?> GetWalletByCurrencyAsync(Guid userId, Currency currency);
    Task<FundWalletResponseDto> FundWalletAsync(Guid userId, FundWalletRequestDto request);
    Task<bool> UpdateWalletBalanceAsync(Guid walletId, decimal amount, bool isDebit = false);
    Task<bool> HoldFundsAsync(Guid walletId, decimal amount);
    Task<bool> ReleaseHoldAsync(Guid walletId, decimal amount);
    Task<string?> GetOrCreateVirtualAccountAsync(Guid userId, Currency currency);
}

