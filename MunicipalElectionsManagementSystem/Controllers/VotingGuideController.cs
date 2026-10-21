using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalElectionsManagementSystem.Data;

namespace MunicipalElectionsManagementSystem.Controllers;

//personal voting guide stored in session, no login required
public class VotingGuideController : Controller
{
    private const string SessionKey = "VotingGuide";

    private readonly ApplicationDbContext _context;

    public VotingGuideController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var ids = GetCandidateIds();

        var candidates = await _context.Candidates
            .AsNoTracking()
            .Include(c => c.Position)
                .ThenInclude(p => p.Municipality)
            .Where(c => ids.Contains(c.CandidateId))
            .ToListAsync();

        return View(candidates);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, string? returnUrl)
    {
        var candidate = await _context.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CandidateId == id);

        if (candidate == null)
        {
            return NotFound();
        }

        var ids = GetCandidateIds();

        if (ids.Add(id))
        {
            SaveCandidateIds(ids);
            TempData["SuccessMessage"] =
                $"{candidate.Name} was added to your voting guide.";
        }
        else
        {
            TempData["ErrorMessage"] =
                $"{candidate.Name} is already in your voting guide.";
        }

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var ids = GetCandidateIds();

        if (ids.Remove(id))
        {
            SaveCandidateIds(ids);
            TempData["SuccessMessage"] =
                "Candidate removed from your voting guide.";
        }

        return RedirectToAction(nameof(Index));
    }

    private HashSet<int> GetCandidateIds()
    {
        var json = HttpContext.Session.GetString(SessionKey);

        return string.IsNullOrEmpty(json)
            ? new HashSet<int>()
            : JsonSerializer.Deserialize<HashSet<int>>(json) ?? new HashSet<int>();
    }

    private void SaveCandidateIds(HashSet<int> ids)
    {
        HttpContext.Session.SetString(
            SessionKey, JsonSerializer.Serialize(ids));
    }
}
