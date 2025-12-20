using FasterXchange.Application.Contracts.Services;
using FasterXchange.Infrastructure.KycProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FasterXchange.Infrastructure.KycProviders;

/// <summary>
/// Factory for creating KYC provider instances
/// Allows easy switching between providers via configuration
/// </summary>
public class KycProviderFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public KycProviderFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public IKycProvider CreateProvider(string? providerName = null)
    {
        // Get provider name from config or use default
        providerName ??= _configuration["Kyc:Provider"] ?? "Sumsub";

        return providerName.ToLower() switch
        {
            "sumsub" => new SumsubKycProvider(
                _loggerFactory.CreateLogger<SumsubKycProvider>(),
                _configuration),
            
            "verifyme" => new VerifyMeKycProvider(
                _loggerFactory.CreateLogger<VerifyMeKycProvider>(),
                _configuration),
            
            "clientsdk" or "client-sdk" => new ClientSdkKycProvider(
                _loggerFactory.CreateLogger<ClientSdkKycProvider>(),
                _configuration),
            
            _ => new SumsubKycProvider( // Default fallback
                _loggerFactory.CreateLogger<SumsubKycProvider>(),
                _configuration)
        };
    }
}

