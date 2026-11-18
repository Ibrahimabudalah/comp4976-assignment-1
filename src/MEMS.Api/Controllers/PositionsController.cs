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
public class PositionsController : ControllerBase
{
    private readonly IPositionRepository _positionRepository;
    private readonly IMunicipalityRepository _municipalityRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public PositionsController(
        IPositionRepository positionRepository,
        IMunicipalityRepository municipalityRepository,
        UserManager<ApplicationUser> userManager)
    {
        _positionRepository = positionRepository;
        _municipalityRepository = municipalityRepository;
        _userManager = userManager;
    }


    // Gets all positions belonging to the specified tenant slug.
    [HttpGet("api/{tenantSlug}/positions")]
    [ProducesResponseType(typeof(List<PositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PositionDto>>> GetByTenant(string tenantSlug)
    {
        var tenant = await _municipalityRepository.GetBySlugAsync(tenantSlug);
        if (tenant == null) return NotFound(new { message = $"Tenant '{tenantSlug}' was not found." });

        var list = await _positionRepository.GetByTenantSlugAsync(tenantSlug);
        return Ok(list.Select(MapToDto).ToList());
    }

    // Gets a single position with tenant isolation check.
    [HttpGet("api/{tenantSlug}/positions/{id:int}")]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PositionDto>> GetByTenantAndId(string tenantSlug, int id)
    {
        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null || !position.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = $"Position with ID {id} does not exist under tenant '{tenantSlug}'." });
        }

        return Ok(MapToDto(position));
    }

    // Creates a new position for the specified tenant.
    [HttpPost("api/{tenantSlug}/positions")]
    [Authorize]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PositionDto>> CreateForTenant(string tenantSlug, [FromBody] CreatePositionDto dto)
    {
        if (!await CanWriteToTenantAsync(tenantSlug))
        {
            return Forbid();
        }

        var tenant = await _municipalityRepository.GetBySlugAsync(tenantSlug);
        if (tenant == null) return NotFound(new { message = $"Tenant '{tenantSlug}' was not found." });

        dto.MunicipalityId = tenant.MunicipalityId;
        return await CreateInternal(dto);
    }

    // Updates a position with tenant isolation check.
    [HttpPut("api/{tenantSlug}/positions/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateForTenant(string tenantSlug, int id, [FromBody] UpdatePositionDto dto)
    {
        if (!await CanWriteToTenantAsync(tenantSlug))
        {
            return Forbid();
        }

        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null || !position.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = $"Position with ID {id} does not exist under tenant '{tenantSlug}'." });
        }

        dto.MunicipalityId = position.MunicipalityId;
        return await UpdateInternal(id, dto);
    }

    // Deletes a position with tenant isolation check.
    [HttpDelete("api/{tenantSlug}/positions/{id:int}")]
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

        var position = await _positionRepository.GetByIdAsync(id);
        if (position == null || !position.Municipality.TenantSlug.Equals(tenantSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = $"Position with ID {id} does not exist under tenant '{tenantSlug}'." });
        }

        return await DeleteInternal(id);
    }


    [HttpGet("api/positions")]
    [ProducesResponseType(typeof(List<PositionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PositionDto>>> GetAll([FromQuery] int? municipalityId)
    {
        var list = await _positionRepository.GetAllAsync(municipalityId);
        return Ok(list.Select(MapToDto).ToList());
    }

    [HttpGet("api/positions/{id:int}")]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PositionDto>> GetById(int id)
    {
        var item = await _positionRepository.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Position with ID {id} was not found." });
        return Ok(MapToDto(item));
    }

    [HttpPost("api/positions")]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionDto dto)
    {
        return await CreateInternal(dto);
    }

    [HttpPut("api/positions/{id:int}")]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePositionDto dto)
    {
        return await UpdateInternal(id, dto);
    }

    [HttpDelete("api/positions/{id:int}")]
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
    private async Task<ActionResult<PositionDto>> CreateInternal(CreatePositionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!await _municipalityRepository.ExistsAsync(dto.MunicipalityId))
        {
            return BadRequest(new { message = "Associated municipality does not exist." });
        }

        if (await _positionRepository.NameExistsInMunicipalityAsync(dto.Name, dto.MunicipalityId))
        {
            return BadRequest(new { message = $"A position named '{dto.Name}' already exists in this municipality." });
        }

        var position = new Position
        {
            Name = dto.Name.Trim(),
            SeatsAvailable = dto.SeatsAvailable,
            MunicipalityId = dto.MunicipalityId
        };

        var created = await _positionRepository.CreateAsync(position);
        var refreshed = await _positionRepository.GetByIdAsync(created.PositionId);
        return CreatedAtAction(nameof(GetById), new { id = created.PositionId }, MapToDto(refreshed ?? created));
    }

    private async Task<IActionResult> UpdateInternal(int id, UpdatePositionDto dto)
    {
        if (id != dto.PositionId) return BadRequest(new { message = "Route ID and Body ID mismatch." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _positionRepository.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = $"Position with ID {id} was not found." });

        if (await _positionRepository.NameExistsInMunicipalityAsync(dto.Name, dto.MunicipalityId, id))
        {
            return BadRequest(new { message = $"A position named '{dto.Name}' already exists in this municipality." });
        }

        existing.Name = dto.Name.Trim();
        existing.SeatsAvailable = dto.SeatsAvailable;
        existing.MunicipalityId = dto.MunicipalityId;

        await _positionRepository.UpdateAsync(existing);
        return NoContent();
    }

    private async Task<IActionResult> DeleteInternal(int id)
    {
        var existing = await _positionRepository.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = $"Position with ID {id} was not found." });

        try
        {
            var deleted = await _positionRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static PositionDto MapToDto(Position p) => new()
    {
        PositionId = p.PositionId,
        Name = p.Name,
        SeatsAvailable = p.SeatsAvailable,
        MunicipalityId = p.MunicipalityId,
        MunicipalityName = p.Municipality?.Name ?? string.Empty,
        TenantSlug = p.Municipality?.TenantSlug ?? string.Empty,
        CandidateCount = p.Candidates?.Count ?? 0
    };
}
