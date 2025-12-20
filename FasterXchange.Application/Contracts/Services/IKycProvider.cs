using FasterXchange.Application.Contracts.DTOs.Kyc;

namespace FasterXchange.Application.Contracts.Services;

/// <summary>
/// Abstract interface for KYC providers (Sumsub, VerifyMe, Client SDK, etc.)
/// This allows easy swapping of KYC vendors without changing business logic
/// </summary>
public interface IKycProvider
{
    /// <summary>
    /// Provider name/identifier
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Initialize KYC verification for a user
    /// Returns access token/URL for client SDK or server-side verification
    /// </summary>
    Task<KycInitiationResult> InitiateVerificationAsync(KycInitiationRequest request);

    /// <summary>
    /// Get verification status from provider
    /// </summary>
    Task<KycStatusResult> GetVerificationStatusAsync(string externalKycId);

    /// <summary>
    /// Process webhook from KYC provider
    /// </summary>
    Task<KycWebhookResult> ProcessWebhookAsync(object webhookPayload, string signature);

    /// <summary>
    /// Verify webhook signature
    /// </summary>
    bool VerifyWebhookSignature(object payload, string signature);

    /// <summary>
    /// Check if provider supports client SDK integration
    /// </summary>
    bool SupportsClientSdk { get; }

    /// <summary>
    /// Get client SDK configuration if supported
    /// </summary>
    Task<ClientSdkConfig?> GetClientSdkConfigAsync(Guid userId);
}

