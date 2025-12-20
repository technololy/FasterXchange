namespace FasterXchange.Application.Contracts.DTOs;

public class UserDto
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
    public bool IsTransactionPinEnabled { get; set; }
    public bool IsBiometricEnabled { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsBalanceVisible { get; set; }
    public string Status { get; set; } = string.Empty;
    public string KycStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

