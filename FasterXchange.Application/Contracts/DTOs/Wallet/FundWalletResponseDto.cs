using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.DTOs.Wallet;

public class FundWalletResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? TransactionId { get; set; }
    public string? ReferenceId { get; set; }
    public string? ExternalReferenceId { get; set; }
    public TransactionStatus Status { get; set; }
    public string? PaymentUrl { get; set; } // For redirects
    public string? VirtualAccountNumber { get; set; } // For NGN virtual account
    public string? VirtualAccountBank { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
}

