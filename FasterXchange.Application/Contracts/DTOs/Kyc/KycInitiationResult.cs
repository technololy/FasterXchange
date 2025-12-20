namespace FasterXchange.Application.Contracts.DTOs.Kyc;

public class KycInitiationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ExternalKycId { get; set; } // Provider's KYC ID
    public string? AccessToken { get; set; } // For client SDK
    public string? VerificationUrl { get; set; } // For server-side redirect
    public ClientSdkConfig? ClientSdkConfig { get; set; } // If using client SDK
    public Dictionary<string, string>? Metadata { get; set; } // Provider-specific data
}

