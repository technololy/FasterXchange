using FasterXchange.Domain.Entities;

namespace FasterXchange.Application.Contracts.Repositories;

public interface IKycRepository
{
    Task<KycDocument?> GetByUserIdAsync(Guid userId);
    Task<KycDocument?> GetByExternalKycIdAsync(string externalKycId);
    Task<KycDocument> CreateAsync(KycDocument kycDocument);
    Task<KycDocument> UpdateAsync(KycDocument kycDocument);
    Task<List<KycDocument>> GetPendingKycAsync();
    Task<List<KycDocument>> GetRejectedKycAsync();
}

