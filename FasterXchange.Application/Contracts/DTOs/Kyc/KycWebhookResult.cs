using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.DTOs.Kyc;

public class KycWebhookResult
{
    public bool Success { get; set; }
    public string? ExternalKycId { get; set; }
    public Guid? UserId { get; set; }
    public KycStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Dictionary<string, object>? ProviderData { get; set; }
}

