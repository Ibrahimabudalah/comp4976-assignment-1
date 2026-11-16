using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// Caches the tenant (municipality) per slug so navigating between tenant pages doesn't re-fetch it.
// Unknown slugs are cached too (as null) so a bad URL doesn't hit the API on every render.
// The in-flight Task is cached, not the result, so a page and TenantShell asking at the same moment share one request.
public class TenantState(MemsApiClient api)
{
    private readonly Dictionary<string, Task<MunicipalityDto?>> _cache = new();

    public async Task<MunicipalityDto?> GetAsync(string slug)
    {
        var key = slug.Trim().ToLowerInvariant();
        if (!_cache.TryGetValue(key, out var request))
            _cache[key] = request = api.GetMunicipalityAsync(key);
        try { return await request; }
        catch
        {
            _cache.Remove(key); // don't cache failures: let the next attempt retry
            throw;
        }
    }
}
