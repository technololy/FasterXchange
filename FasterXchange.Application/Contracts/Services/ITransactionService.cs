using FasterXchange.Application.Contracts.DTOs.Wallet;
using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Services;

public interface ITransactionService
{
    Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId);
    Task<TransactionHistoryResponseDto> GetTransactionHistoryAsync(Guid userId, TransactionHistoryRequestDto request);
    Task<Domain.Entities.Transaction> CreateTransactionAsync(
        Guid userId,
        Guid? walletId,
        TransactionType type,
        Currency currency,
        decimal amount,
        decimal? fee = null,
        string? description = null,
        string? externalReferenceId = null);
    Task<bool> UpdateTransactionStatusAsync(Guid transactionId, TransactionStatus status, string? failureReason = null);
    Task<bool> CompleteTransactionAsync(Guid transactionId, string? externalReferenceId = null);
}

