namespace FasterXchange.Application.Contracts.DTOs.Kyc;

public class ClientSdkConfig
{
    public string ProviderName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? Environment { get; set; } // sandbox, production
    public Dictionary<string, string>? AdditionalConfig { get; set; }
    public string? SdkUrl { get; set; } // CDN URL for SDK
    public string? DocumentationUrl { get; set; }
}

