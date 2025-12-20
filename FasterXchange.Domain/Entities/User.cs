namespace FasterXchange.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? CountryOfResidence { get; set; }
    public string? PreferredCurrency { get; set; }
    public string? Language { get; set; }
    public string? PasswordHash { get; set; } // Optional password
    public string? TransactionPinHash { get; set; }
    public bool IsTransactionPinEnabled { get; set; }
    public bool IsBiometricEnabled { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsBalanceVisible { get; set; } = true;
    public UserStatus Status { get; set; } = UserStatus.Unverified;
    public KycStatus KycStatus { get; set; } = KycStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsFrozen { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }

    // Navigation properties
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual KycDocument? KycDocument { get; set; }
    public virtual UserSettings? Settings { get; set; }
    public virtual NotificationPreferences? NotificationPreferences { get; set; }
}

public enum UserStatus
{
    Unverified,
    Verified,
    Suspended,
    Deactivated
}

public enum KycStatus
{
    NotStarted,
    Pending,
    Approved,
    Rejected,
    ResubmissionRequired
}

