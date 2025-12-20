namespace FasterXchange.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? WalletId { get; set; }
    public TransactionType Type { get; set; }
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal? Fee { get; set; }
    public decimal? ExchangeRate { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? ReferenceId { get; set; }
    public string? ExternalReferenceId { get; set; } // Partner reference
    public string? Description { get; set; }
    public string? SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? InteracEmail { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Wallet? Wallet { get; set; }
    public virtual Remittance? Remittance { get; set; }
    public virtual Escrow? Escrow { get; set; }
}

public enum TransactionType
{
    WalletFunding,
    WalletWithdrawal,
    RemittanceOutbound,
    RemittanceInbound,
    P2PExchange,
    EscrowDeposit,
    EscrowRelease,
    Refund
}

public enum TransactionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Reversed
}

