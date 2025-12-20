namespace FasterXchange.Application.Contracts.DTOs.Wallet;

public class WalletBalanceDto
{
    public Guid WalletId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OnHold { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? VirtualAccountNumber { get; set; }
    public string? VirtualAccountBank { get; set; }
}

