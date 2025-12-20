namespace FasterXchange.Domain.Entities;

public class NotificationPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool TransferNotifications { get; set; } = true;
    public bool EscrowNotifications { get; set; } = true;
    public bool RateAlerts { get; set; } = false;
    public bool SecurityAlerts { get; set; } = true; // Cannot be disabled
    public bool MarketingMessages { get; set; } = false;
    public bool PushEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}

