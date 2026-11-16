using System.Net.Http.Json;
using System.Net;
using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// Read-only calls to MEMS.Api. Never pass voting-guide selections through here.
public class MemsApiClient(HttpClient http)
{
    public async Task<List<MunicipalityDto>> GetMunicipalitiesAsync()
        => await http.GetFromJsonAsync<List<MunicipalityDto>>("api/municipalities") ?? [];

    public Task<MunicipalityDto?> GetMunicipalityAsync(string slug)
        => GetOrNullAsync<MunicipalityDto>($"api/municipalities/by-slug/{Uri.EscapeDataString(slug)}");

    public async Task<List<PositionDto>> GetPositionsAsync(string slug)
        => await http.GetFromJsonAsync<List<PositionDto>>($"api/{Uri.EscapeDataString(slug)}/positions") ?? [];

    public async Task<List<CandidateDto>> GetCandidatesAsync(string slug, string? search = null, string? position = null)
    {
        var url = $"api/{Uri.EscapeDataString(slug)}/candidates?search={Uri.EscapeDataString(search ?? "")}&position={Uri.EscapeDataString(position ?? "")}";
        return await http.GetFromJsonAsync<List<CandidateDto>>(url) ?? [];
    }

    public Task<CandidateDto?> GetCandidateAsync(string slug, int id)
        => GetOrNullAsync<CandidateDto>($"api/{Uri.EscapeDataString(slug)}/candidates/{id}");

    // 404 means "not found" (null); any other failure throws so pages can show ErrorAlert.
    private async Task<T?> GetOrNullAsync<T>(string url) where T : class
    {
        try { return await http.GetFromJsonAsync<T>(url); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { return null; }
    }
}
