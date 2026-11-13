using Microsoft.EntityFrameworkCore;
using MEMS.Data.Entities;

namespace MEMS.Data.Repositories;

public class MunicipalityRepository : IMunicipalityRepository
{
    private readonly ApplicationDbContext _context;

    public MunicipalityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Municipality>> GetAllAsync()
    {
        return await _context.Municipalities
            .Include(m => m.Positions)
            .Include(m => m.Candidates)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Municipality?> GetByIdAsync(int id)
    {
        return await _context.Municipalities
            .Include(m => m.Positions)
            .Include(m => m.Candidates)
            .FirstOrDefaultAsync(m => m.MunicipalityId == id);
    }

    public async Task<Municipality?> GetBySlugAsync(string slug)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return await _context.Municipalities
            .Include(m => m.Positions)
                .ThenInclude(p => p.Candidates)
            .FirstOrDefaultAsync(m => m.TenantSlug.ToLower() == normalized);
    }

    public async Task<Municipality> CreateAsync(Municipality municipality)
    {
        municipality.TenantSlug = municipality.TenantSlug.Trim().ToLowerInvariant();
        _context.Municipalities.Add(municipality);
        await _context.SaveChangesAsync();
        return municipality;
    }

    public async Task UpdateAsync(Municipality municipality)
    {
        municipality.TenantSlug = municipality.TenantSlug.Trim().ToLowerInvariant();
        _context.Entry(municipality).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var municipality = await _context.Municipalities
            .Include(m => m.Positions)
            .Include(m => m.Candidates)
            .Include(m => m.Users)
            .FirstOrDefaultAsync(m => m.MunicipalityId == id);

        if (municipality == null) return false;

        // Prevent deletion if dependent items exist
        if (municipality.Positions.Any() || municipality.Candidates.Any() || municipality.Users.Any())
        {
            throw new InvalidOperationException("Cannot delete municipality with existing positions, candidates, or assigned administrators.");
        }

        _context.Municipalities.Remove(municipality);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Municipalities.AnyAsync(m => m.MunicipalityId == id);
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return await _context.Municipalities.AnyAsync(m =>
            m.TenantSlug.ToLower() == normalized && m.MunicipalityId != excludeId);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToUpperInvariant();
        return await _context.Municipalities.AnyAsync(m =>
            m.Name.ToUpper() == normalized && m.MunicipalityId != excludeId);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Municipalities.CountAsync();
    }
}
