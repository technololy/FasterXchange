using FasterXchange.Application.Contracts.DTOs.Kyc;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FasterXchange.Infrastructure.KycProviders;

/// <summary>
/// Base class for KYC providers - provides common functionality
/// </summary>
public abstract class BaseKycProvider : IKycProvider
{
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected BaseKycProvider(ILogger logger, IConfiguration configuration)
    {
        Logger = logger;
        Configuration = configuration;
    }

    public abstract string ProviderName { get; }
    public abstract bool SupportsClientSdk { get; }

    public abstract Task<KycInitiationResult> InitiateVerificationAsync(KycInitiationRequest request);
    public abstract Task<KycStatusResult> GetVerificationStatusAsync(string externalKycId);
    public abstract Task<KycWebhookResult> ProcessWebhookAsync(object webhookPayload, string signature);
    public abstract bool VerifyWebhookSignature(object payload, string signature);

    public virtual async Task<ClientSdkConfig?> GetClientSdkConfigAsync(Guid userId)
    {
        if (!SupportsClientSdk)
            return null;

        // Default implementation - override in derived classes
        return await Task.FromResult<ClientSdkConfig?>(null);
    }

    protected virtual KycStatus MapProviderStatusToKycStatus(string providerStatus)
    {
        // Default mapping - override in derived classes
        return providerStatus.ToLower() switch
        {
            "approved" or "verified" or "passed" => KycStatus.Approved,
            "rejected" or "declined" or "failed" => KycStatus.Rejected,
            "pending" or "review" or "processing" => KycStatus.Pending,
            "resubmission" or "retry" => KycStatus.ResubmissionRequired,
            _ => KycStatus.Pending
        };
    }
}

