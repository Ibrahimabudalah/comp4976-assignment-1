using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// Caches the tenant (municipality) per slug so navigating between tenant pages doesn't re-fetch it.
// Unknown slugs are cached too (as null) so a bad URL doesn't hit the API on every render.
public class TenantState(MemsApiClient api)
{
    private readonly Dictionary<string, MunicipalityDto?> _cache = new();

    public async Task<MunicipalityDto?> GetAsync(string slug)
    {
        var key = slug.Trim().ToLowerInvariant();
        if (!_cache.TryGetValue(key, out var tenant))
            _cache[key] = tenant = await api.GetMunicipalityAsync(key);
        return tenant;
    }
}
