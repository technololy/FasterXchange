using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.DTOs.Wallet;

public class FundWalletRequestDto
{
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
    public FundingMethod Method { get; set; }
    
    // For card payments
    public string? CardToken { get; set; }
    public string? PaymentMethodId { get; set; }
    
    // For bank transfers
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? SenderName { get; set; }
    
    // For Interac
    public string? InteracEmail { get; set; }
}

public enum FundingMethod
{
    DebitCard,      // Stripe
    BankTransfer,   // Interac for CAD, Bank transfer for NGN
    VirtualAccount  // NGN only - bank transfer to virtual account
}

