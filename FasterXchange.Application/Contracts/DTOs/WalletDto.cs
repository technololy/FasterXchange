namespace FasterXchange.Application.Contracts.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OnHold { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? VirtualAccountNumber { get; set; }
    public string? VirtualAccountBank { get; set; }
}

