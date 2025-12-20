using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.DTOs.Wallet;

public class TransactionHistoryRequestDto
{
    public Currency? Currency { get; set; }
    public TransactionType? Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

