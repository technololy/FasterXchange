using FasterXchange.Application.Contracts.DTOs.Kyc;

namespace FasterXchange.Application.Contracts.Services;

/// <summary>
/// Main KYC service interface - uses IKycProvider abstraction
/// </summary>
public interface IKycService
{
    /// <summary>
    /// Start KYC verification process
    /// </summary>
    Task<StartKycResponseDto> StartKycVerificationAsync(Guid userId, StartKycRequestDto request);

    /// <summary>
    /// Get current KYC status for user
    /// </summary>
    Task<KycStatusDto> GetKycStatusAsync(Guid userId);

    /// <summary>
    /// Resubmit KYC if rejected
    /// </summary>
    Task<StartKycResponseDto> ResubmitKycAsync(Guid userId, StartKycRequestDto request);

    /// <summary>
    /// Process webhook from KYC provider
    /// </summary>
    Task<bool> ProcessKycWebhookAsync(string providerName, object payload, string signature);

    /// <summary>
    /// Check if user has completed KYC
    /// </summary>
    Task<bool> IsKycApprovedAsync(Guid userId);

    /// <summary>
    /// Update KYC status from provider
    /// </summary>
    Task<bool> UpdateKycStatusFromProviderAsync(string externalKycId);
}

