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
    private readonly IMunicipalityRepository _municipalityRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IMunicipalityRepository municipalityRepository,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
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
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
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

        var keyString = _configuration["Jwt:Key"] ?? "MEMS_SuperSecret_Key_For_Jwt_Auth_Token_Signing_2026_COMP4976!";
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
