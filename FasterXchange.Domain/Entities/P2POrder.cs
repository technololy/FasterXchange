namespace FasterXchange.Domain.Entities;

public class P2POrder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderType Type { get; set; }
    public Currency BuyCurrency { get; set; }
    public Currency SellCurrency { get; set; }
    public decimal Amount { get; set; }
    public decimal DesiredRate { get; set; }
    public decimal? MinRate { get; set; }
    public decimal? MaxRate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public bool AutoMatch { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? MatchedAt { get; set; }
    public Guid? MatchedWithOrderId { get; set; }
    public Guid? MatchedWithUserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Escrow? Escrow { get; set; }
}

public enum OrderType
{
    BuyNGNWithCAD,
    SellNGNForCAD
}

public enum OrderStatus
{
    Open,
    Matched,
    Cancelled,
    Expired,
    Completed
}

