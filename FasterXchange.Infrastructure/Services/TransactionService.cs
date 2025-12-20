using FasterXchange.Application.Contracts.DTOs.Wallet;
using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FasterXchange.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdAndUserIdAsync(transactionId, userId);
        if (transaction == null)
            return null;

        return MapToDto(transaction);
    }

    public async Task<TransactionHistoryResponseDto> GetTransactionHistoryAsync(
        Guid userId,
        TransactionHistoryRequestDto request)
    {
        var transactions = await _transactionRepository.GetUserTransactionsAsync(
            userId,
            request.Currency,
            request.Type,
            request.Status,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize);

        var totalCount = await _transactionRepository.GetUserTransactionCountAsync(
            userId,
            request.Currency,
            request.Type,
            request.Status,
            request.StartDate,
            request.EndDate);

        return new TransactionHistoryResponseDto
        {
            Transactions = transactions.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<Domain.Entities.Transaction> CreateTransactionAsync(
        Guid userId,
        Guid? walletId,
        TransactionType type,
        Currency currency,
        decimal amount,
        decimal? fee = null,
        string? description = null,
        string? externalReferenceId = null)
    {
        var referenceId = GenerateReferenceId();

        var transaction = new Domain.Entities.Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletId = walletId,
            Type = type,
            Currency = currency,
            Amount = amount,
            Fee = fee,
            Description = description,
            ReferenceId = referenceId,
            ExternalReferenceId = externalReferenceId,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return await _transactionRepository.CreateAsync(transaction);
    }

    public async Task<bool> UpdateTransactionStatusAsync(
        Guid transactionId,
        TransactionStatus status,
        string? failureReason = null)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            return false;

        transaction.Status = status;
        transaction.FailureReason = failureReason;

        if (status == TransactionStatus.Completed)
        {
            transaction.CompletedAt = DateTime.UtcNow;
        }

        await _transactionRepository.UpdateAsync(transaction);
        return true;
    }

    public async Task<bool> CompleteTransactionAsync(
        Guid transactionId,
        string? externalReferenceId = null)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            return false;

        transaction.Status = TransactionStatus.Completed;
        transaction.CompletedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(externalReferenceId))
        {
            transaction.ExternalReferenceId = externalReferenceId;
        }

        await _transactionRepository.UpdateAsync(transaction);
        return true;
    }

    private TransactionDto MapToDto(Domain.Entities.Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            Type = transaction.Type.ToString(),
            Currency = transaction.Currency.ToString(),
            Amount = transaction.Amount,
            Fee = transaction.Fee,
            ExchangeRate = transaction.ExchangeRate,
            Status = transaction.Status.ToString(),
            ReferenceId = transaction.ReferenceId,
            Description = transaction.Description,
            SenderName = transaction.SenderName,
            ReceiverName = transaction.ReceiverName,
            CreatedAt = transaction.CreatedAt,
            CompletedAt = transaction.CompletedAt,
            EstimatedDeliveryAt = transaction.EstimatedDeliveryAt
        };
    }

    private string GenerateReferenceId()
    {
        return $"TXN{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

