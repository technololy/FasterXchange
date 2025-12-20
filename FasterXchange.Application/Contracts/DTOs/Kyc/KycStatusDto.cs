namespace FasterXchange.Application.Contracts.DTOs.Kyc;

public class KycStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? ExternalKycId { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool CanResubmit { get; set; }
}

