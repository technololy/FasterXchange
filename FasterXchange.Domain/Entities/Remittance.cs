namespace FasterXchange.Domain.Entities;

public class Remittance
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid UserId { get; set; }
    public Currency SourceCurrency { get; set; }
    public Currency DestinationCurrency { get; set; }
    public decimal SourceAmount { get; set; }
    public decimal DestinationAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Fee { get; set; }
    public RemittanceStatus Status { get; set; } = RemittanceStatus.Pending;
    public string? RateLockId { get; set; }
    public DateTime? RateLockExpiresAt { get; set; }
    public string? BeneficiaryAccountNumber { get; set; }
    public string? BeneficiaryBankName { get; set; }
    public string? BeneficiaryBankCode { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? InteracEmail { get; set; }
    public string? PartnerReferenceId { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public enum RemittanceStatus
{
    Pending,
    RateLocked,
    PaymentInitiated,
    Processing,
    Completed,
    Failed,
    Cancelled
}

