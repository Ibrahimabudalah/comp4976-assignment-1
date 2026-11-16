using Blazored.SessionStorage;
using MEMS.Shared.Dtos;

namespace MEMS.Public.Services;

// One saved pick. The position id is stored with the candidate id so the seat rule needs no extra lookups.
public record GuideEntry(int CandidateId, int PositionId);

// The personal voting guide. Election privacy (spec): picks live ONLY in this browser tab's sessionStorage.
// This class must never depend on MemsApiClient or HttpClient, so a pick can never be sent to the server.
public class VotingGuideService(ISessionStorageService storage, ILogger<VotingGuideService> logger)
{
    // in-memory copy per tenant (one tab = one service instance = one sessionStorage), loaded once; the Task is
    // cached so many GuideButtons asking at the same moment trigger a single read
    private readonly Dictionary<string, Task<List<GuideEntry>>> _cache = new();

    public event Action<string>? Changed;   // argument: tenant slug

    private static string Key(string slug) => $"mems-guide:{slug.Trim().ToLowerInvariant()}";

    public async Task<IReadOnlyList<GuideEntry>> GetAsync(string slug) => (await LoadAsync(slug)).ToList();

    // The ballot rules. Pure function: easy to reason about and to test.
    public static VoterBallotValidationResult CanAdd(string slug, CandidateDto candidate, int seats, IReadOnlyCollection<GuideEntry> current)
    {
        if (current.Any(e => e.CandidateId == candidate.CandidateId)) return VoterBallotValidationResult.Success();
        if (!string.Equals(candidate.TenantSlug, slug, StringComparison.OrdinalIgnoreCase))
            return VoterBallotValidationResult.Failure("This candidate belongs to another municipality.");
        if (current.Count(e => e.PositionId == candidate.PositionId) >= seats)
            return VoterBallotValidationResult.Failure($"You can choose up to {seats} candidate{(seats == 1 ? "" : "s")} for {candidate.PositionName}. Remove one first.");
        return VoterBallotValidationResult.Success();
    }

    public async Task<VoterBallotValidationResult> AddAsync(string slug, CandidateDto candidate, int seats)
    {
        var list = await LoadAsync(slug);
        var result = CanAdd(slug, candidate, seats, list);
        if (!result.IsValid || list.Any(e => e.CandidateId == candidate.CandidateId)) return result;
        list.Add(new GuideEntry(candidate.CandidateId, candidate.PositionId));
        return await SaveAsync(slug, list) ? result : VoterBallotValidationResult.Failure("Your browser is blocking storage, so the guide can't be saved.");
    }

    public async Task RemoveAsync(string slug, int candidateId)
    {
        var list = await LoadAsync(slug);
        if (list.RemoveAll(e => e.CandidateId == candidateId) > 0) await SaveAsync(slug, list);
    }

    public async Task ClearAsync(string slug)
    {
        var list = await LoadAsync(slug);
        list.Clear();
        await SaveAsync(slug, list);
    }

    // Used by the guide page to drop candidates an admin has since deleted.
    public async Task ReplaceAsync(string slug, IEnumerable<GuideEntry> entries)
    {
        var list = await LoadAsync(slug);
        list.Clear();
        list.AddRange(entries);
        await SaveAsync(slug, list);
    }

    private Task<List<GuideEntry>> LoadAsync(string slug)
    {
        var key = Key(slug);
        if (!_cache.TryGetValue(key, out var task)) _cache[key] = task = ReadAsync(key);
        return task;
    }

    private async Task<List<GuideEntry>> ReadAsync(string key)
    {
        // entries without a real candidate id (hand-edited or unreadable storage) are ignored so they can't count toward seat limits
        try { return (await storage.GetItemAsync<List<GuideEntry>>(key) ?? []).Where(e => e.CandidateId > 0).ToList(); }
        catch (Exception ex)   // storage blocked or holds something unreadable: start with an empty guide
        {
            logger.LogWarning(ex, "Could not read the voting guide from session storage");
            return [];
        }
    }

    private async Task<bool> SaveAsync(string slug, List<GuideEntry> list)
    {
        try
        {
            await storage.SetItemAsync(Key(slug), list);
            Changed?.Invoke(slug.Trim().ToLowerInvariant());
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not save the voting guide to session storage");
            return false;
        }
    }
}
