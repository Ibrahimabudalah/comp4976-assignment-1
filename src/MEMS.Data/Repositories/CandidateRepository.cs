using Microsoft.EntityFrameworkCore;
using MEMS.Data.Entities;

namespace MEMS.Data.Repositories;

public class CandidateRepository : ICandidateRepository
{
    private readonly ApplicationDbContext _context;

    public CandidateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Candidate>> GetAllAsync(int? municipalityId = null, int? positionId = null, string? search = null)
    {
        var query = _context.Candidates
            .Include(c => c.Position)
            .Include(c => c.Municipality)
            .AsQueryable();

        if (municipalityId.HasValue)
        {
            query = query.Where(c => c.MunicipalityId == municipalityId.Value);
        }

        if (positionId.HasValue)
        {
            query = query.Where(c => c.PositionId == positionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%"));
        }

        return await query
            .OrderBy(c => c.Municipality.Name)
            .ThenBy(c => c.Position.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Candidate>> GetByTenantSlugAsync(string tenantSlug, string? search = null, string? positionName = null)
    {
        var normalizedSlug = tenantSlug.Trim().ToLowerInvariant();
        var query = _context.Candidates
            .Include(c => c.Position)
            .Include(c => c.Municipality)
            .Where(c => c.Municipality.TenantSlug.ToLower() == normalizedSlug);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(positionName))
        {
            query = query.Where(c => c.Position.Name == positionName);
        }

        return await query
            .OrderBy(c => c.Position.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Candidate?> GetByIdAsync(int id)
    {
        return await _context.Candidates
            .Include(c => c.Position)
            .Include(c => c.Municipality)
            .FirstOrDefaultAsync(c => c.CandidateId == id);
    }

    public async Task<List<Candidate>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        return await _context.Candidates
            .Include(c => c.Position)
            .Include(c => c.Municipality)
            .Where(c => idList.Contains(c.CandidateId))
            .OrderBy(c => c.Position.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Candidate> CreateAsync(Candidate candidate)
    {
        // Enforce data integrity: Candidate's MunicipalityId must match Position's MunicipalityId
        var position = await _context.Positions.FindAsync(candidate.PositionId);
        if (position == null)
        {
            throw new ArgumentException("Position does not exist.");
        }
        candidate.MunicipalityId = position.MunicipalityId;

        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();
        return candidate;
    }

    public async Task UpdateAsync(Candidate candidate)
    {
        var position = await _context.Positions.FindAsync(candidate.PositionId);
        if (position == null)
        {
            throw new ArgumentException("Position does not exist.");
        }
        candidate.MunicipalityId = position.MunicipalityId;

        _context.Entry(candidate).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var candidate = await _context.Candidates.FindAsync(id);
        if (candidate == null) return false;

        _context.Candidates.Remove(candidate);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Candidates.AnyAsync(c => c.CandidateId == id);
    }

    public async Task<int> GetCountAsync(int? municipalityId = null)
    {
        if (municipalityId.HasValue)
        {
            return await _context.Candidates.CountAsync(c => c.MunicipalityId == municipalityId.Value);
        }
        return await _context.Candidates.CountAsync();
    }
}
