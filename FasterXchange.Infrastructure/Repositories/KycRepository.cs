using FasterXchange.Application.Contracts.Repositories;
using FasterXchange.Domain.Entities;
using FasterXchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FasterXchange.Infrastructure.Repositories;

public class KycRepository : IKycRepository
{
    private readonly ApplicationDbContext _context;

    public KycRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KycDocument?> GetByUserIdAsync(Guid userId)
    {
        return await _context.KycDocuments
            .FirstOrDefaultAsync(k => k.UserId == userId);
    }

    public async Task<KycDocument?> GetByExternalKycIdAsync(string externalKycId)
    {
        return await _context.KycDocuments
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.ExternalKycId == externalKycId);
    }

    public async Task<KycDocument> CreateAsync(KycDocument kycDocument)
    {
        kycDocument.SubmittedAt = DateTime.UtcNow;
        _context.KycDocuments.Add(kycDocument);
        await _context.SaveChangesAsync();
        return kycDocument;
    }

    public async Task<KycDocument> UpdateAsync(KycDocument kycDocument)
    {
        _context.KycDocuments.Update(kycDocument);
        await _context.SaveChangesAsync();
        return kycDocument;
    }

    public async Task<List<KycDocument>> GetPendingKycAsync()
    {
        return await _context.KycDocuments
            .Where(k => k.Status == KycStatus.Pending)
            .Include(k => k.User)
            .ToListAsync();
    }

    public async Task<List<KycDocument>> GetRejectedKycAsync()
    {
        return await _context.KycDocuments
            .Where(k => k.Status == KycStatus.Rejected || k.Status == KycStatus.ResubmissionRequired)
            .Include(k => k.User)
            .ToListAsync();
    }
}

