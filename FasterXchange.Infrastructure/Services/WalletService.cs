using FasterXchange.Application.Contracts.DTOs.Wallet;
using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FasterXchange.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionService _transactionService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository walletRepository,
        IUserRepository userRepository,
        ITransactionService transactionService,
        IPaymentService paymentService,
        ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _userRepository = userRepository;
        _transactionService = transactionService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<List<WalletBalanceDto>> GetUserWalletsAsync(Guid userId)
    {
        var wallets = await _walletRepository.GetUserWalletsAsync(userId);
        return wallets.Select(MapToDto).ToList();
    }

    public async Task<WalletBalanceDto?> GetWalletByCurrencyAsync(Guid userId, Currency currency)
    {
        var wallet = await _walletRepository.GetByUserIdAndCurrencyAsync(userId, currency);
        if (wallet == null)
            return null;

        return MapToDto(wallet);
    }

    public async Task<FundWalletResponseDto> FundWalletAsync(Guid userId, FundWalletRequestDto request)
    {
        try
        {
            // Get or create wallet
            var wallet = await _walletRepository.GetByUserIdAndCurrencyAsync(userId, request.Currency);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Currency = request.Currency,
                    Balance = 0,
                    OnHold = 0,
                    CreatedAt = DateTime.UtcNow
                };
                wallet = await _walletRepository.CreateAsync(wallet);
            }

            // Create transaction record
            var transaction = await _transactionService.CreateTransactionAsync(
                userId,
                wallet.Id,
                TransactionType.WalletFunding,
                request.Currency,
                request.Amount,
                description: $"Wallet funding via {request.Method}");

            PaymentResult? paymentResult = null;

            // Process payment based on method
            switch (request.Method)
            {
                case FundingMethod.DebitCard:
                    if (request.Currency != Currency.CAD)
                    {
                        return new FundWalletResponseDto
                        {
                            Success = false,
                            Message = "Card payments are only supported for CAD",
                            TransactionId = transaction.Id,
                            Status = TransactionStatus.Failed
                        };
                    }

                    if (string.IsNullOrEmpty(request.CardToken) && string.IsNullOrEmpty(request.PaymentMethodId))
                    {
                        return new FundWalletResponseDto
                        {
                            Success = false,
                            Message = "Card token or payment method ID is required",
                            TransactionId = transaction.Id,
                            Status = TransactionStatus.Failed
                        };
                    }

                    paymentResult = await _paymentService.ProcessCardPaymentAsync(
                        request.Currency,
                        request.Amount,
                        request.CardToken ?? request.PaymentMethodId!,
                        request.PaymentMethodId);

                    break;

                case FundingMethod.BankTransfer:
                    if (request.Currency == Currency.CAD)
                    {
                        // Interac e-Transfer
                        if (string.IsNullOrEmpty(request.InteracEmail))
                        {
                            return new FundWalletResponseDto
                            {
                                Success = false,
                                Message = "Interac email is required for CAD bank transfers",
                                TransactionId = transaction.Id,
                                Status = TransactionStatus.Failed
                            };
                        }

                        paymentResult = await _paymentService.ProcessInteracPaymentAsync(
                            request.Amount,
                            request.InteracEmail);
                    }
                    else
                    {
                        // NGN bank transfer - requires virtual account
                        var virtualAccount = await GetOrCreateVirtualAccountAsync(userId, Currency.NGN);
                        if (string.IsNullOrEmpty(virtualAccount))
                        {
                            return new FundWalletResponseDto
                            {
                                Success = false,
                                Message = "Failed to create virtual account",
                                TransactionId = transaction.Id,
                                Status = TransactionStatus.Failed
                            };
                        }

                        return new FundWalletResponseDto
                        {
                            Success = true,
                            Message = "Please transfer funds to the virtual account",
                            TransactionId = transaction.Id,
                            ReferenceId = transaction.ReferenceId,
                            Status = TransactionStatus.Pending,
                            VirtualAccountNumber = wallet.VirtualAccountNumber,
                            VirtualAccountBank = wallet.VirtualAccountBank,
                            EstimatedDeliveryAt = DateTime.UtcNow.AddHours(1)
                        };
                    }
                    break;

                case FundingMethod.VirtualAccount:
                    if (request.Currency != Currency.NGN)
                    {
                        return new FundWalletResponseDto
                        {
                            Success = false,
                            Message = "Virtual account is only available for NGN",
                            TransactionId = transaction.Id,
                            Status = TransactionStatus.Failed
                        };
                    }

                    var va = await GetOrCreateVirtualAccountAsync(userId, Currency.NGN);
                    return new FundWalletResponseDto
                    {
                        Success = true,
                        Message = "Please transfer funds to the virtual account",
                        TransactionId = transaction.Id,
                        ReferenceId = transaction.ReferenceId,
                        Status = TransactionStatus.Pending,
                        VirtualAccountNumber = wallet.VirtualAccountNumber,
                        VirtualAccountBank = wallet.VirtualAccountBank,
                        EstimatedDeliveryAt = DateTime.UtcNow.AddHours(1)
                    };
            }

            if (paymentResult == null)
            {
                return new FundWalletResponseDto
                {
                    Success = false,
                    Message = "Payment processing failed",
                    TransactionId = transaction.Id,
                    Status = TransactionStatus.Failed
                };
            }

            // Update transaction with external reference
            if (!string.IsNullOrEmpty(paymentResult.ExternalReferenceId))
            {
                await _transactionService.CompleteTransactionAsync(
                    transaction.Id,
                    paymentResult.ExternalReferenceId);
            }

            if (paymentResult.Success)
            {
                // Update wallet balance
                await UpdateWalletBalanceAsync(wallet.Id, request.Amount, isDebit: false);
                await _transactionService.UpdateTransactionStatusAsync(
                    transaction.Id,
                    TransactionStatus.Completed);
            }
            else
            {
                await _transactionService.UpdateTransactionStatusAsync(
                    transaction.Id,
                    TransactionStatus.Failed,
                    paymentResult.FailureReason);
            }

            return new FundWalletResponseDto
            {
                Success = paymentResult.Success,
                Message = paymentResult.Success ? "Wallet funded successfully" : paymentResult.FailureReason,
                TransactionId = transaction.Id,
                ReferenceId = transaction.ReferenceId,
                ExternalReferenceId = paymentResult.ExternalReferenceId,
                Status = paymentResult.Success ? TransactionStatus.Completed : TransactionStatus.Failed,
                PaymentUrl = paymentResult.PaymentUrl,
                EstimatedDeliveryAt = paymentResult.EstimatedDeliveryAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error funding wallet for user {UserId}", userId);
            return new FundWalletResponseDto
            {
                Success = false,
                Message = "An error occurred while processing the funding request"
            };
        }
    }

    public async Task<bool> UpdateWalletBalanceAsync(Guid walletId, decimal amount, bool isDebit = false)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet == null)
            return false;

        if (isDebit)
        {
            if (wallet.AvailableBalance < amount)
                return false; // Insufficient funds

            wallet.Balance -= amount;
        }
        else
        {
            wallet.Balance += amount;
        }

        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);
        return true;
    }

    public async Task<bool> HoldFundsAsync(Guid walletId, decimal amount)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet == null)
            return false;

        if (wallet.AvailableBalance < amount)
            return false; // Insufficient available balance

        wallet.OnHold += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);
        return true;
    }

    public async Task<bool> ReleaseHoldAsync(Guid walletId, decimal amount)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet == null)
            return false;

        if (wallet.OnHold < amount)
            return false; // Cannot release more than on hold

        wallet.OnHold -= amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);
        return true;
    }

    public async Task<string?> GetOrCreateVirtualAccountAsync(Guid userId, Currency currency)
    {
        if (currency != Currency.NGN)
            return null;

        var wallet = await _walletRepository.GetByUserIdAndCurrencyAsync(userId, currency);
        if (wallet == null)
        {
            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Currency = currency,
                Balance = 0,
                OnHold = 0,
                CreatedAt = DateTime.UtcNow
            };
            wallet = await _walletRepository.CreateAsync(wallet);
        }

        // If virtual account already exists, return it
        if (!string.IsNullOrEmpty(wallet.VirtualAccountNumber))
            return wallet.VirtualAccountNumber;

        // Create virtual account via payment service
        var user = await _userRepository.GetByIdAsync(wallet.UserId);
        if (user == null)
            return null;

        // Call payment service to create virtual account
        var paymentResult = await _paymentService.CreateVirtualAccountAsync(
            wallet.UserId,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            user.FullName ?? "User");

        if (paymentResult.Success && !string.IsNullOrEmpty(paymentResult.VirtualAccountNumber))
        {
            wallet.VirtualAccountNumber = paymentResult.VirtualAccountNumber;
            wallet.VirtualAccountBank = paymentResult.VirtualAccountBank ?? "Flutterwave Bank";
            await _walletRepository.UpdateAsync(wallet);
            return paymentResult.VirtualAccountNumber;
        }

        // Fallback: Generate a placeholder virtual account number
        var virtualAccountNumber = $"VA{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        wallet.VirtualAccountNumber = virtualAccountNumber;
        wallet.VirtualAccountBank = "Flutterwave Bank"; // Placeholder

        await _walletRepository.UpdateAsync(wallet);
        return virtualAccountNumber;
    }

    private WalletBalanceDto MapToDto(Wallet wallet)
    {
        return new WalletBalanceDto
        {
            WalletId = wallet.Id,
            Currency = wallet.Currency.ToString(),
            Balance = wallet.Balance,
            OnHold = wallet.OnHold,
            AvailableBalance = wallet.AvailableBalance,
            VirtualAccountNumber = wallet.VirtualAccountNumber,
            VirtualAccountBank = wallet.VirtualAccountBank
        };
    }
}

