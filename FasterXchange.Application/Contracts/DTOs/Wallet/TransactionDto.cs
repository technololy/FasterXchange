namespace FasterXchange.Application.Contracts.DTOs.Wallet;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid? WalletId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? Fee { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Description { get; set; }
    public string? SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
}

