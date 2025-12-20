namespace FasterXchange.Domain.Entities;

public class Escrow
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Currency BuyCurrency { get; set; }
    public Currency SellCurrency { get; set; }
    public decimal BuyAmount { get; set; }
    public decimal SellAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public EscrowStatus Status { get; set; } = EscrowStatus.AwaitingDeposit;
    public bool BuyerFunded { get; set; } = false;
    public bool SellerFunded { get; set; } = false;
    public DateTime? BuyerFundedAt { get; set; }
    public DateTime? SellerFundedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    
    // Navigation properties
    public virtual P2POrder Order { get; set; } = null!;
    public virtual User Buyer { get; set; } = null!;
    public virtual User Seller { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum EscrowStatus
{
    AwaitingDeposit,
    BuyerFunded,
    SellerFunded,
    BothFunded,
    Released,
    Cancelled,
    Disputed
}

