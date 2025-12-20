namespace FasterXchange.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Currency Currency { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal OnHold { get; set; } = 0; // Escrow funds
    public decimal AvailableBalance => Balance - OnHold;
    public string? VirtualAccountNumber { get; set; } // For NGN wallet
    public string? VirtualAccountBank { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum Currency
{
    CAD,
    NGN
}

