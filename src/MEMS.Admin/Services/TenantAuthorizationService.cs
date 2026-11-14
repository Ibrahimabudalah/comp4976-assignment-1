using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Shared.Constants;

namespace MEMS.Admin.Services;

public class TenantAuthorizationService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public TenantAuthorizationService(
        AuthenticationStateProvider authStateProvider,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _authStateProvider = authStateProvider;
        _userManager = userManager;
        _context = context;
    }

    public async Task<ClaimsPrincipal> GetCurrentUserAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        return state.User;
    }

    public async Task<ApplicationUser?> GetCurrentApplicationUserAsync()
    {
        var principal = await GetCurrentUserAsync();
        if (principal.Identity?.IsAuthenticated != true) return null;
        return await _userManager.GetUserAsync(principal);
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        var principal = await GetCurrentUserAsync();
        return principal.Identity?.IsAuthenticated == true && principal.IsInRole(AdminRoles.SuperAdmin);
    }

    public async Task<int?> GetAssignedMunicipalityIdAsync()
    {
        if (await IsSuperAdminAsync()) return null; // Super admin has no single municipality constraint
        var user = await GetCurrentApplicationUserAsync();
        return user?.MunicipalityId;
    }

    public async Task<bool> CanManageMunicipalityAsync(int municipalityId)
    {
        if (await IsSuperAdminAsync()) return true;

        var user = await GetCurrentApplicationUserAsync();
        if (user == null || user.MunicipalityId != municipalityId) return false;

        var municipality = await _context.Municipalities.FindAsync(municipalityId);
        if (municipality == null) return false;

        var principal = await GetCurrentUserAsync();
        return principal.IsInRole(AdminRoles.ForMunicipality(municipality.Name))
            || principal.IsInRole(AdminRoles.MunicipalityAdmin);
    }
}
