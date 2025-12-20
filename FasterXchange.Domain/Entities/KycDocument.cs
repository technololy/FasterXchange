namespace FasterXchange.Domain.Entities;

public class KycDocument
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string IdType { get; set; } = string.Empty; // Passport, DriverLicense, NationalID
    public string? IdNumber { get; set; }
    public string? ExternalKycId { get; set; } // Sumsub/VerifyMe ID
    public KycStatus Status { get; set; } = KycStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}

