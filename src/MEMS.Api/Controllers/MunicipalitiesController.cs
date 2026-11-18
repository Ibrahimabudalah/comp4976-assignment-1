using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MEMS.Data.Entities;
using MEMS.Data.Repositories;
using MEMS.Shared.Constants;
using MEMS.Shared.Dtos;

namespace MEMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MunicipalitiesController : ControllerBase
{
    private readonly IMunicipalityRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<MunicipalitiesController> _logger;

    public MunicipalitiesController(
        IMunicipalityRepository repository,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<MunicipalitiesController> logger)
    {
        _repository = repository;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    // Gets all registered municipalities
    [HttpGet]
    [ProducesResponseType(typeof(List<MunicipalityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MunicipalityDto>>> GetAll()
    {
        var list = await _repository.GetAllAsync();
        return Ok(list.Select(MapToDto).ToList());
    }

    // Gets a municipality by its ID.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MunicipalityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MunicipalityDto>> GetById(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Municipality with ID {id} was not found." });
        return Ok(MapToDto(item));
    }

    // Gets tenant metadata by its URL tenant slug
    [HttpGet("by-slug/{tenantSlug}")]
    [ProducesResponseType(typeof(MunicipalityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MunicipalityDto>> GetBySlug(string tenantSlug)
    {
        var item = await _repository.GetBySlugAsync(tenantSlug);
        if (item == null) return NotFound(new { message = $"Tenant '{tenantSlug}' was not found." });
        return Ok(MapToDto(item));
    }

    // Creates a new municipality tenant.
    [HttpPost]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(typeof(MunicipalityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MunicipalityDto>> Create([FromBody] CreateMunicipalityDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _repository.NameExistsAsync(dto.Name))
        {
            return BadRequest(new { message = $"A municipality with the name '{dto.Name}' already exists." });
        }

        if (await _repository.SlugExistsAsync(dto.TenantSlug))
        {
            return BadRequest(new { message = $"The tenant slug '{dto.TenantSlug}' is already taken." });
        }

        var municipality = new Municipality
        {
            Name = dto.Name.Trim(),
            TenantSlug = dto.TenantSlug.Trim().ToLowerInvariant(),
            Province = dto.Province.Trim(),
            ElectionDate = dto.ElectionDate,
            Description = dto.Description?.Trim(),
            LogoFileName = dto.LogoFileName
        };

        var created = await _repository.CreateAsync(municipality);

        // Ensure municipality specific admin role is created with checked IdentityResult
        var roleName = AdminRoles.ForMunicipality(created.Name);
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to create role {RoleName}: {Errors}",
                    roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        // Create initial municipality admin user if requested
        if (dto.CreateInitialAdmin && !string.IsNullOrWhiteSpace(dto.AdminUserName) && !string.IsNullOrWhiteSpace(dto.AdminPassword))
        {
            var user = new ApplicationUser
            {
                UserName = dto.AdminUserName.Trim(),
                FullName = dto.AdminFullName?.Trim(),
                Email = $"{dto.AdminUserName.Trim().ToLower()}@{created.TenantSlug}.local",
                MunicipalityId = created.MunicipalityId
            };

            var userResult = await _userManager.CreateAsync(user, dto.AdminPassword);
            if (userResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleName);
                await _userManager.AddToRoleAsync(user, AdminRoles.MunicipalityAdmin);
            }
            else
            {
                _logger.LogWarning("Failed to create initial admin for {Name}: {Errors}",
                    created.Name, string.Join(", ", userResult.Errors.Select(e => e.Description)));
            }
        }

        return CreatedAtAction(nameof(GetById), new { id = created.MunicipalityId }, MapToDto(created));
    }

    // Updates an existing municipality tenant.
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMunicipalityDto dto)
    {
        if (id != dto.MunicipalityId) return BadRequest(new { message = "Route ID and Body ID mismatch." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = $"Municipality with ID {id} was not found." });

        // Enforce tenant boundary: Super Admin can update any tenant; Municipality Admin can only update their own
        if (!User.IsInRole(AdminRoles.SuperAdmin))
        {
            var claimSlug = User.FindFirst("tenantSlug")?.Value;
            if (claimSlug == null || !claimSlug.Equals(existing.TenantSlug, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }

        if (await _repository.NameExistsAsync(dto.Name, id))
        {
            return BadRequest(new { message = $"A municipality with the name '{dto.Name}' already exists." });
        }

        if (await _repository.SlugExistsAsync(dto.TenantSlug, id))
        {
            return BadRequest(new { message = $"The tenant slug '{dto.TenantSlug}' is already taken." });
        }

        var oldRoleName = AdminRoles.ForMunicipality(existing.Name);

        existing.Name = dto.Name.Trim();
        existing.TenantSlug = dto.TenantSlug.Trim().ToLowerInvariant();
        existing.Province = dto.Province.Trim();
        existing.ElectionDate = dto.ElectionDate;
        existing.Description = dto.Description?.Trim();
        if (!string.IsNullOrEmpty(dto.LogoFileName))
        {
            existing.LogoFileName = dto.LogoFileName;
        }

        await _repository.UpdateAsync(existing);

        // Keep role name synchronized if name changed
        var newRoleName = AdminRoles.ForMunicipality(existing.Name);
        if (oldRoleName != newRoleName)
        {
            var role = await _roleManager.FindByNameAsync(oldRoleName);
            if (role != null)
            {
                await _roleManager.SetRoleNameAsync(role, newRoleName);
                var updateResult = await _roleManager.UpdateAsync(role);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to update role name to {NewName}", newRoleName);
                }
            }
        }

        return NoContent();
    }

    // Deletes a municipality tenant if it has no associated positions, candidates, or admins.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AdminRoles.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = $"Municipality with ID {id} was not found." });

        try
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return NotFound();

            var role = await _roleManager.FindByNameAsync(AdminRoles.ForMunicipality(existing.Name));
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static MunicipalityDto MapToDto(Municipality m) => new()
    {
        MunicipalityId = m.MunicipalityId,
        Name = m.Name,
        TenantSlug = m.TenantSlug,
        Province = m.Province,
        ElectionDate = m.ElectionDate,
        Description = m.Description,
        LogoFileName = m.LogoFileName,
        PositionCount = m.Positions?.Count ?? 0,
        CandidateCount = m.Candidates?.Count ?? 0
    };
}
