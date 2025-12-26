namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class SetPinRequestDto
{
    public string Pin { get; set; } = string.Empty;
    public string ConfirmPin { get; set; } = string.Empty;
}
