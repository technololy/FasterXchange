namespace FasterXchange.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Identifier { get; set; } = string.Empty; // Email or phone
    public string Code { get; set; } = string.Empty;
    public OtpType Type { get; set; }
    public OtpStatus Status { get; set; } = OtpStatus.Pending;
    public int Attempts { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Navigation property
    public virtual User? User { get; set; }
}

public enum OtpType
{
    Registration,
    Login,
    EmailVerification,
    PhoneVerification,
    PasswordReset,
    PinReset,
    Transaction
}

public enum OtpStatus
{
    Pending,
    Verified,
    Expired,
    Failed
}

