using FasterXchange.Application.Contracts.DTOs.Wallet;
using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Services;

public interface IPaymentService
{
    Task<PaymentResult> ProcessCardPaymentAsync(
        Currency currency,
        decimal amount,
        string cardToken,
        string? paymentMethodId = null);
    
    Task<PaymentResult> ProcessInteracPaymentAsync(
        decimal amount,
        string interacEmail);
    
    Task<PaymentResult> CreateVirtualAccountAsync(
        Guid userId,
        string email,
        string phoneNumber,
        string fullName);
    
    Task<PaymentResult> VerifyBankTransferAsync(
        string referenceId,
        Currency currency);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? ExternalReferenceId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? VirtualAccountNumber { get; set; }
    public string? VirtualAccountBank { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
}

