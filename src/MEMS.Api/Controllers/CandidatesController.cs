using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MEMS.Data.Entities;
using MEMS.Data.Repositories;
using MEMS.Shared.Constants;
using MEMS.Shared.Dtos;

namespace MEMS.Api.Controllers;

[ApiController]
[Produces("application/json")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IMunicipalityRepository _municipalityRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public CandidatesController(
        ICandidateRepository candidateRepository,
        IPositionRepository positionRepository,
        IMunicipalityRepository municipalityRepository,
        UserManager<ApplicationUser> userManager)
    {
        _candidateRepository = candidateRepository;
        _positionRepository = positionRepository;
        _municipalityRepository = municipalityRepository;
        _userManager = userManager;
    }

    // Gets all candidates for the specified tenant with optional name search and position filtering.
    [HttpGet("api/{tenantSlug}/candidates")]
    [ProducesResponseType(typeof(List<CandidateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CandidateDto>>> GetByTenant(
        string tenantSlug,
        [FromQuery] string? search = null,
        [FromQuery] string? position = null)
    {
        if (!await CanReadTenantAsync(tenantSlug)) return Forbid();

        var tenant = await _municipalityRepository.GetBySlugAsync(tenantSlug);
        if (tenant == null) return NotFound(new { message = $"Tenant '{tenantSlug}' was not found." });

        var list = await _candidateRepository.GetByTenantSlugAsync(tenantSlug, search, position);
        return Ok(list.Select(MapToDto).ToList());
    }

    // Gets a single candidate ensuring they belong to the specified tenant.
    [HttpGet("api/{tenantSlug}/candidates/{id:int}")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateDto>> GetByTenantAndId(string tenantSlug, int id)
    {
        if (!await CanReadTenantAsync(tenantSlug)) return Forbid();

        var candidate = await _candidateRepository.GetByIdAsync(id);
        if (candidate == null || !candidate.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = $"Candidate with ID {id} does not exist under tenant '{tenantSlug}'." });
        }

        return Ok(MapToDto(candidate));
    }

    // Compares 2 or 3 candidates belonging strictly to the specified tenant.
    [HttpPost("api/{tenantSlug}/candidates/compare")]
    [ProducesResponseType(typeof(CandidateComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateComparisonDto>> CompareForTenant(string tenantSlug, [FromBody] int[] candidateIds)
    {
        if (!await CanReadTenantAsync(tenantSlug)) return Forbid();

        var tenant = await _municipalityRepository.GetBySlugAsync(tenantSlug);
        if (tenant == null) return NotFound(new { message = $"Tenant '{tenantSlug}' was not found." });

        var distinctIds = candidateIds?.Distinct().ToList() ?? new List<int>();
        if (distinctIds.Count < 2 || distinctIds.Count > 3)
        {
            return BadRequest(new { message = "You must select 2 or 3 candidates to compare." });
        }

        var candidates = await _candidateRepository.GetByIdsAsync(distinctIds);
        if (candidates.Count != distinctIds.Count)
        {
            return BadRequest(new { message = "One or more requested candidate IDs could not be found." });
        }

        // Enforce tenant boundary: all compared candidates must belong to the active tenant
        if (candidates.Any(c => !c.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { message = "Cross-tenant candidate comparisons are strictly forbidden." });
        }

        return Ok(new CandidateComparisonDto
        {
            Candidates = candidates.Select(MapToDto).ToList()
        });
    }

    // Creates a candidate for the specified tenant.
    [HttpPost("api/{tenantSlug}/candidates")]
    [Authorize]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateDto>> CreateForTenant(string tenantSlug, [FromBody] CreateCandidateDto dto)
    {
        if (!await CanWriteToTenantAsync(tenantSlug))
        {
            return Forbid();
        }

        var tenant = await _municipalityRepository.GetBySlugAsync(tenantSlug);
        if (tenant == null) return NotFound(new { message = $"Tenant '{tenantSlug}' was not found." });

        var position = await _positionRepository.GetByIdAsync(dto.PositionId);
        if (position == null || position.MunicipalityId != tenant.MunicipalityId)
        {
            return BadRequest(new { message = $"Position {dto.PositionId} does not belong to tenant '{tenantSlug}'." });
        }

        return await CreateInternal(dto);
    }

    // Updates a candidate ensuring tenant boundary isolation.
    [HttpPut("api/{tenantSlug}/candidates/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateForTenant(string tenantSlug, int id, [FromBody] UpdateCandidateDto dto)
    {
        if (!await CanWriteToTenantAsync(tenantSlug))
        {
            return Forbid();
        }

        var candidate = await _candidateRepository.GetByIdAsync(id);
        if (candidate == null || !candidate.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = $"Candidate with ID {id} does not exist under tenant '{tenantSlug}'." });
        }

        return await UpdateInternal(id, dto);
    }

    // Deletes a candidate ensuring tenant boundary isolation.
    [HttpDelete("api/{tenantSlug}/candidates/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteForTenant(string tenantSlug, int id)
    {
        if (!await CanWriteToTenantAsync(tenantSlug))
        {
            return Forbid();
        }

        var candidate = await _candidateRepository.GetByIdAsync(id);
        if (candidate == null || !candidate.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = $"Candidate with ID {id} does not exist under tenant '{tenantSlug}'." });
        }

        return await DeleteInternal(id);
    }


    [HttpGet("api/candidates")]
    [ProducesResponseType(typeof(List<CandidateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CandidateDto>>> GetAll(
        [FromQuery] int? municipalityId,
        [FromQuery] int? positionId,
        [FromQuery] string? search)
    {
        if (!TryScopeToOwnMunicipality(ref municipalityId)) return Forbid();

        var list = await _candidateRepository.GetAllAsync(municipalityId, positionId, search);
        return Ok(list.Select(MapToDto).ToList());
    }

    [HttpGet("api/candidates/{id:int}")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateDto>> GetById(int id)
    {
        var item = await _candidateRepository.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Candidate with ID {id} was not found." });
        if (!await CanReadTenantAsync(item.Municipality.TenantSlug)) return Forbid();
        return Ok(MapToDto(item));
    }

    [HttpPost("api/candidates")]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CandidateDto>> Create([FromBody] CreateCandidateDto dto)
    {
        return await CreateInternal(dto);
    }

    [HttpPut("api/candidates/{id:int}")]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCandidateDto dto)
    {
        return await UpdateInternal(id, dto);
    }

    [HttpDelete("api/candidates/{id:int}")]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        return await DeleteInternal(id);
    }

    // Tenant read scoping: anonymous callers (the public site) may read any tenant; a signed-in admin only their own.
    private async Task<bool> CanReadTenantAsync(string tenantSlug) =>
        User.Identity?.IsAuthenticated != true || await CanWriteToTenantAsync(tenantSlug);

    // For the global list routes: a signed-in Municipality Admin is limited to their own municipality.
    // Returns false when they ask for another municipality (or have no tenant claim).
    private bool TryScopeToOwnMunicipality(ref int? municipalityId)
    {
        if (User.Identity?.IsAuthenticated != true || User.IsInRole(AdminRoles.SuperAdmin)) return true;
        if (!int.TryParse(User.FindFirst("municipalityId")?.Value, out var own)) return false;
        if (municipalityId.HasValue && municipalityId.Value != own) return false;
        municipalityId = own;
        return true;
    }

    // Tenant write scoping authorization helper
    private async Task<bool> CanWriteToTenantAsync(string tenantSlug)
    {
        if (User.IsInRole(AdminRoles.SuperAdmin))
            return true;

        var claimSlug = User.FindFirst("tenantSlug")?.Value;
        if (!string.IsNullOrEmpty(claimSlug))
        {
            return claimSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase);
        }

        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName)) return false;

        var user = await _userManager.FindByNameAsync(userName);
        if (user?.MunicipalityId == null) return false;

        var tenant = await _municipalityRepository.GetByIdAsync(user.MunicipalityId.Value);
        return tenant != null && tenant.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase);
    }

    // Shared implementation helpers
    private async Task<ActionResult<CandidateDto>> CreateInternal(CreateCandidateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var position = await _positionRepository.GetByIdAsync(dto.PositionId);
        if (position == null)
        {
            return BadRequest(new { message = "Selected position does not exist." });
        }

        var candidate = new Candidate
        {
            Name = dto.Name.Trim(),
            Profile = dto.Profile.Trim(),
            PhotoFileName = dto.PhotoFileName,
            Website = dto.Website?.Trim(),
            Email = dto.Email?.Trim(),
            PositionId = dto.PositionId,
            MunicipalityId = position.MunicipalityId,
            DateCreated = DateTime.UtcNow
        };

        var created = await _candidateRepository.CreateAsync(candidate);
        var refreshed = await _candidateRepository.GetByIdAsync(created.CandidateId);
        return CreatedAtAction(nameof(GetById), new { id = created.CandidateId }, MapToDto(refreshed ?? created));
    }

    private async Task<IActionResult> UpdateInternal(int id, UpdateCandidateDto dto)
    {
        if (id != dto.CandidateId) return BadRequest(new { message = "Route ID and Body ID mismatch." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _candidateRepository.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = $"Candidate with ID {id} was not found." });

        var position = await _positionRepository.GetByIdAsync(dto.PositionId);
        if (position == null)
        {
            return BadRequest(new { message = "Selected position does not exist." });
        }

        existing.Name = dto.Name.Trim();
        existing.Profile = dto.Profile.Trim();
        existing.PhotoFileName = dto.PhotoFileName;
        existing.Website = dto.Website?.Trim();
        existing.Email = dto.Email?.Trim();
        existing.PositionId = dto.PositionId;
        existing.MunicipalityId = position.MunicipalityId;

        await _candidateRepository.UpdateAsync(existing);
        return NoContent();
    }

    private async Task<IActionResult> DeleteInternal(int id)
    {
        var existing = await _candidateRepository.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = $"Candidate with ID {id} was not found." });

        var deleted = await _candidateRepository.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    private static CandidateDto MapToDto(Candidate c) => new()
    {
        CandidateId = c.CandidateId,
        Name = c.Name,
        Profile = c.Profile,
        PhotoFileName = c.PhotoFileName,
        Website = c.Website,
        Email = c.Email,
        PositionId = c.PositionId,
        PositionName = c.Position?.Name ?? string.Empty,
        MunicipalityId = c.MunicipalityId,
        MunicipalityName = c.Municipality?.Name ?? string.Empty,
        TenantSlug = c.Municipality?.TenantSlug ?? string.Empty,
        DateCreated = c.DateCreated
    };
}
