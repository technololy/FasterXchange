using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByIdAndUserIdAsync(Guid id, Guid userId);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<Transaction> UpdateAsync(Transaction transaction);
    Task<List<Transaction>> GetUserTransactionsAsync(
        Guid userId,
        Currency? currency = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20);
    Task<int> GetUserTransactionCountAsync(
        Guid userId,
        Currency? currency = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<Transaction?> GetByReferenceIdAsync(string referenceId);
    Task<Transaction?> GetByExternalReferenceIdAsync(string externalReferenceId);
}

