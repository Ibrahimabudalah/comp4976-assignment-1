using MEMS.Data.Entities;

namespace MEMS.Data.Repositories;

public interface IMunicipalityRepository
{
    Task<List<Municipality>> GetAllAsync();
    Task<Municipality?> GetByIdAsync(int id);
    Task<Municipality?> GetBySlugAsync(string slug);
    Task<Municipality> CreateAsync(Municipality municipality);
    Task UpdateAsync(Municipality municipality);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<int> GetCountAsync();
}
