using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// Caches the tenant (municipality) per slug so navigating between tenant pages doesn't re-fetch it.
// Unknown slugs are cached too (as null) so a bad URL doesn't hit the API on every render.
// The in-flight Task is cached, not the result, so a page and TenantShell asking at the same moment share one request.
public class TenantState(MemsApiClient api)
{
    private readonly Dictionary<string, Task<MunicipalityDto?>> _cache = new();
    private readonly Dictionary<string, Task<List<PositionDto>>> _positions = new();
    private Task<List<MunicipalityDto>>? _all;

    // All municipalities (for the municipality filter), fetched once per tab.
    public async Task<List<MunicipalityDto>> GetAllAsync()
    {
        _all ??= api.GetMunicipalitiesAsync();
        try { return await _all; }
        catch
        {
            _all = null;
            throw;
        }
    }

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

    // Positions (and their seat counts) per tenant, shared by every page and GuideButton for this tab.
    public async Task<List<PositionDto>> GetPositionsAsync(string slug)
    {
        var key = slug.Trim().ToLowerInvariant();
        if (!_positions.TryGetValue(key, out var request))
            _positions[key] = request = api.GetPositionsAsync(key);
        try { return await request; }
        catch
        {
            _positions.Remove(key);
            throw;
        }
    }
}
