using MEMS.Data.Entities;

namespace MEMS.Data.Repositories;

public interface ICandidateRepository
{
    Task<List<Candidate>> GetAllAsync(int? municipalityId = null, int? positionId = null, string? search = null);
    Task<List<Candidate>> GetByTenantSlugAsync(string tenantSlug, string? search = null, string? positionName = null);
    Task<Candidate?> GetByIdAsync(int id);
    Task<List<Candidate>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Candidate> CreateAsync(Candidate candidate);
    Task UpdateAsync(Candidate candidate);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetCountAsync(int? municipalityId = null);
}
