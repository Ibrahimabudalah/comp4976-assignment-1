using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MEMS.Data.Entities;
using MEMS.Data.Repositories;
using MEMS.Shared.Dtos;

namespace MEMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMunicipalityRepository _municipalityRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMunicipalityRepository municipalityRepository,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _municipalityRepository = municipalityRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates user credentials and returns a JWT Bearer token with role and tenant claims.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for unknown user: {UserName}", dto.UserName);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        if (!user.LockoutEnabled)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out due to repeated failed attempts: {UserName}", dto.UserName);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for user: {UserName}", dto.UserName);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        string? tenantSlug = null;
        if (user.MunicipalityId.HasValue)
        {
            var municipality = await _municipalityRepository.GetByIdAsync(user.MunicipalityId.Value);
            tenantSlug = municipality?.TenantSlug;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!string.IsNullOrEmpty(tenantSlug))
        {
            claims.Add(new Claim("tenantSlug", tenantSlug));
        }

        if (user.MunicipalityId.HasValue)
        {
            claims.Add(new Claim("municipalityId", user.MunicipalityId.Value.ToString()));
        }

        var keyString = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "MEMS.Api",
            audience: _configuration["Jwt:Audience"] ?? "MEMS.Clients",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return Ok(new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToList(),
            MunicipalityId = user.MunicipalityId,
            TenantSlug = tenantSlug,
            Expiration = expires
        });
    }
}
