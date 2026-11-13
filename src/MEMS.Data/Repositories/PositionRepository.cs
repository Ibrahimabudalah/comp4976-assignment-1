using Microsoft.EntityFrameworkCore;
using MEMS.Data.Entities;

namespace MEMS.Data.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly ApplicationDbContext _context;

    public PositionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Position>> GetAllAsync(int? municipalityId = null)
    {
        var query = _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .AsQueryable();

        if (municipalityId.HasValue)
        {
            query = query.Where(p => p.MunicipalityId == municipalityId.Value);
        }

        return await query
            .OrderBy(p => p.Municipality.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Position>> GetByTenantSlugAsync(string tenantSlug)
    {
        var normalized = tenantSlug.Trim().ToLowerInvariant();
        return await _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .Where(p => p.Municipality.TenantSlug.ToLower() == normalized)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Position?> GetByIdAsync(int id)
    {
        return await _context.Positions
            .Include(p => p.Municipality)
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.PositionId == id);
    }

    public async Task<Position> CreateAsync(Position position)
    {
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();
        return position;
    }

    public async Task UpdateAsync(Position position)
    {
        _context.Entry(position).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var position = await _context.Positions
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.PositionId == id);

        if (position == null) return false;

        if (position.Candidates.Any())
        {
            throw new InvalidOperationException("Cannot delete position that has registered candidates.");
        }

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Positions.AnyAsync(p => p.PositionId == id);
    }

    public async Task<bool> NameExistsInMunicipalityAsync(string name, int municipalityId, int? excludeId = null)
    {
        var normalized = name.Trim().ToUpperInvariant();
        return await _context.Positions.AnyAsync(p =>
            p.MunicipalityId == municipalityId &&
            p.Name.ToUpper() == normalized &&
            p.PositionId != excludeId);
    }

    public async Task<int> GetCountAsync(int? municipalityId = null)
    {
        if (municipalityId.HasValue)
        {
            return await _context.Positions.CountAsync(p => p.MunicipalityId == municipalityId.Value);
        }
        return await _context.Positions.CountAsync();
    }
}
