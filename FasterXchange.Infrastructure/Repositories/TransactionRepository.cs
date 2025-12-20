using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _context.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<List<Transaction>> GetUserTransactionsAsync(
        Guid userId,
        Currency? currency = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.Transactions
            .Include(t => t.Wallet)
            .Where(t => t.UserId == userId);

        if (currency.HasValue)
            query = query.Where(t => t.Currency == currency.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUserTransactionCountAsync(
        Guid userId,
        Currency? currency = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.Transactions
            .Where(t => t.UserId == userId);

        if (currency.HasValue)
            query = query.Where(t => t.Currency == currency.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedAt <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<Transaction?> GetByReferenceIdAsync(string referenceId)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.ReferenceId == referenceId);
    }

    public async Task<Transaction?> GetByExternalReferenceIdAsync(string externalReferenceId)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.ExternalReferenceId == externalReferenceId);
    }
}

