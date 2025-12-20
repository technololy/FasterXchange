namespace FasterXchange.Domain.Entities;

public class UserSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Language { get; set; } = "en";
    public string CurrencyFormat { get; set; } = "en-CA";
    public string TimeZone { get; set; } = "America/Toronto";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}

