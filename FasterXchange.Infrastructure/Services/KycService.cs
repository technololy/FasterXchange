using FasterXchange.Application.Contracts.DTOs.Kyc;
using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Application.Contracts.Services;
using FasterXchange.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FasterXchange.Infrastructure.Services;

public class KycService : IKycService
{
    private readonly IKycRepository _kycRepository;
    private readonly IUserRepository _userRepository;
    private readonly IKycProvider _kycProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KycService> _logger;

    public KycService(
        IKycRepository kycRepository,
        IUserRepository userRepository,
        IKycProvider kycProvider,
        IConfiguration configuration,
        ILogger<KycService> logger)
    {
        _kycRepository = kycRepository;
        _userRepository = userRepository;
        _kycProvider = kycProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<StartKycResponseDto> StartKycVerificationAsync(Guid userId, StartKycRequestDto request)
    {
        try
        {
            // Get user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new StartKycResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if KYC already exists
            var existingKyc = await _kycRepository.GetByUserIdAsync(userId);
            if (existingKyc != null && existingKyc.Status == KycStatus.Approved)
            {
                return new StartKycResponseDto
                {
                    Success = false,
                    Message = "KYC already approved"
                };
            }

            // Prepare initiation request
            var initiationRequest = new KycInitiationRequest
            {
                UserId = userId,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                CountryCode = request.CountryCode,
                IdType = request.IdType,
                UseClientSdk = request.UseClientSdk
            };

            // Initiate KYC with provider
            var initiationResult = await _kycProvider.InitiateVerificationAsync(initiationRequest);
            if (!initiationResult.Success)
            {
                return new StartKycResponseDto
                {
                    Success = false,
                    Message = initiationResult.Message ?? "Failed to initiate KYC verification"
                };
            }

            // Create or update KYC document
            KycDocument kycDocument;
            if (existingKyc != null)
            {
                // Update existing
                existingKyc.FullName = request.FullName;
                existingKyc.DateOfBirth = request.DateOfBirth;
                existingKyc.Address = request.Address;
                existingKyc.IdType = request.IdType ?? "Passport";
                existingKyc.ExternalKycId = initiationResult.ExternalKycId;
                existingKyc.Status = KycStatus.Pending;
                existingKyc.SubmittedAt = DateTime.UtcNow;
                existingKyc.RejectionReason = null;
                kycDocument = await _kycRepository.UpdateAsync(existingKyc);
            }
            else
            {
                // Create new
                kycDocument = new KycDocument
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    IdType = request.IdType ?? "Passport",
                    ExternalKycId = initiationResult.ExternalKycId,
                    Status = KycStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                };
                kycDocument = await _kycRepository.CreateAsync(kycDocument);
            }

            // Update user KYC status
            user.KycStatus = KycStatus.Pending;
            await _userRepository.UpdateAsync(user);

            return new StartKycResponseDto
            {
                Success = true,
                Message = "KYC verification initiated successfully",
                ExternalKycId = initiationResult.ExternalKycId,
                AccessToken = initiationResult.AccessToken,
                VerificationUrl = initiationResult.VerificationUrl,
                ClientSdkConfig = initiationResult.ClientSdkConfig,
                KycStatus = kycDocument.Status.ToString(),
                Metadata = initiationResult.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting KYC verification for user {UserId}", userId);
            return new StartKycResponseDto
            {
                Success = false,
                Message = "An error occurred while initiating KYC verification"
            };
        }
    }

    public async Task<KycStatusDto> GetKycStatusAsync(Guid userId)
    {
        var kycDocument = await _kycRepository.GetByUserIdAsync(userId);
        if (kycDocument == null)
        {
            return new KycStatusDto
            {
                Status = KycStatus.NotStarted.ToString(),
                CanResubmit = false
            };
        }

        return new KycStatusDto
        {
            Status = kycDocument.Status.ToString(),
            ExternalKycId = kycDocument.ExternalKycId,
            RejectionReason = kycDocument.RejectionReason,
            SubmittedAt = kycDocument.SubmittedAt,
            ReviewedAt = kycDocument.ApprovedAt ?? kycDocument.RejectedAt,
            CanResubmit = kycDocument.Status == KycStatus.Rejected || 
                         kycDocument.Status == KycStatus.ResubmissionRequired
        };
    }

    public async Task<StartKycResponseDto> ResubmitKycAsync(Guid userId, StartKycRequestDto request)
    {
        var existingKyc = await _kycRepository.GetByUserIdAsync(userId);
        if (existingKyc == null)
        {
            return new StartKycResponseDto
            {
                Success = false,
                Message = "No existing KYC found. Please start a new KYC verification."
            };
        }

        if (existingKyc.Status != KycStatus.Rejected && 
            existingKyc.Status != KycStatus.ResubmissionRequired)
        {
            return new StartKycResponseDto
            {
                Success = false,
                Message = "KYC cannot be resubmitted in current status"
            };
        }

        // Start new verification (same as StartKycVerificationAsync)
        return await StartKycVerificationAsync(userId, request);
    }

    public async Task<bool> ProcessKycWebhookAsync(string providerName, object payload, string signature)
    {
        try
        {
            // Get the appropriate provider (in a real implementation, you'd have a provider factory)
            // For now, we use the injected provider
            if (!_kycProvider.VerifyWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Invalid webhook signature from provider {ProviderName}", providerName);
                return false;
            }

            var webhookResult = await _kycProvider.ProcessWebhookAsync(payload, signature);
            if (!webhookResult.Success || string.IsNullOrEmpty(webhookResult.ExternalKycId))
            {
                return false;
            }

            // Find KYC document by external ID
            var kycDocument = await _kycRepository.GetByExternalKycIdAsync(webhookResult.ExternalKycId);
            if (kycDocument == null)
            {
                _logger.LogWarning("KYC document not found for external ID {ExternalKycId}", webhookResult.ExternalKycId);
                return false;
            }

            // Update KYC status
            kycDocument.Status = webhookResult.Status;
            kycDocument.RejectionReason = webhookResult.RejectionReason;

            if (webhookResult.Status == KycStatus.Approved)
            {
                kycDocument.ApprovedAt = webhookResult.ReviewedAt ?? DateTime.UtcNow;
            }
            else if (webhookResult.Status == KycStatus.Rejected)
            {
                kycDocument.RejectedAt = webhookResult.ReviewedAt ?? DateTime.UtcNow;
            }

            await _kycRepository.UpdateAsync(kycDocument);

            // Update user KYC status
            if (kycDocument.UserId != null)
            {
                var user = await _userRepository.GetByIdAsync(kycDocument.UserId);
                if (user != null)
                {
                    user.KycStatus = webhookResult.Status;
                    await _userRepository.UpdateAsync(user);
                }
            }

            _logger.LogInformation("KYC status updated via webhook: {ExternalKycId} -> {Status}", 
                webhookResult.ExternalKycId, webhookResult.Status);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing KYC webhook from {ProviderName}", providerName);
            return false;
        }
    }

    public async Task<bool> IsKycApprovedAsync(Guid userId)
    {
        var kycDocument = await _kycRepository.GetByUserIdAsync(userId);
        return kycDocument != null && kycDocument.Status == KycStatus.Approved;
    }

    public async Task<bool> UpdateKycStatusFromProviderAsync(string externalKycId)
    {
        try
        {
            var statusResult = await _kycProvider.GetVerificationStatusAsync(externalKycId);
            if (!statusResult.Success)
            {
                return false;
            }

            var kycDocument = await _kycRepository.GetByExternalKycIdAsync(externalKycId);
            if (kycDocument == null)
            {
                return false;
            }

            kycDocument.Status = statusResult.Status;
            kycDocument.RejectionReason = statusResult.RejectionReason;

            if (statusResult.Status == KycStatus.Approved && statusResult.ReviewedAt.HasValue)
            {
                kycDocument.ApprovedAt = statusResult.ReviewedAt.Value;
            }
            else if (statusResult.Status == KycStatus.Rejected && statusResult.ReviewedAt.HasValue)
            {
                kycDocument.RejectedAt = statusResult.ReviewedAt.Value;
            }

            await _kycRepository.UpdateAsync(kycDocument);

            // Update user status
            var user = await _userRepository.GetByIdAsync(kycDocument.UserId);
            if (user != null)
            {
                user.KycStatus = statusResult.Status;
                await _userRepository.UpdateAsync(user);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KYC status from provider for {ExternalKycId}", externalKycId);
            return false;
        }
    }
}

