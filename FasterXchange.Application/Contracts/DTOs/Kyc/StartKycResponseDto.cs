namespace FasterXchange.Application.Contracts.DTOs.Kyc;

public class StartKycResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ExternalKycId { get; set; }
    public string? AccessToken { get; set; } // For client SDK
    public string? VerificationUrl { get; set; } // For server-side redirect
    public ClientSdkConfig? ClientSdkConfig { get; set; }
    public string KycStatus { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

