namespace FasterXchange.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; } // iOS, Android, Web
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsTrusted { get; set; } = false;
    public string? BiometricKey { get; set; }
    public DateTime? TrustedAt { get; set; }
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual User User { get; set; } = null!;
}
