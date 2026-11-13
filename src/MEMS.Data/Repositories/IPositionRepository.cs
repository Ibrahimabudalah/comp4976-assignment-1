using MEMS.Data.Entities;

namespace MEMS.Data.Repositories;

public interface IPositionRepository
{
    Task<List<Position>> GetAllAsync(int? municipalityId = null);
    Task<List<Position>> GetByTenantSlugAsync(string tenantSlug);
    Task<Position?> GetByIdAsync(int id);
    Task<Position> CreateAsync(Position position);
    Task UpdateAsync(Position position);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsInMunicipalityAsync(string name, int municipalityId, int? excludeId = null);
    Task<int> GetCountAsync(int? municipalityId = null);
}
