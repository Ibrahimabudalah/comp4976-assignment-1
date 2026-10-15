
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MunicipalElectionsManagementSystem.Models;

namespace MunicipalElectionsManagementSystem.Services;

public class MunicipalityAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public MunicipalityAuthorizationService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> CanManageMunicipalityAsync(
        ClaimsPrincipal principal,
        int municipalityId)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var user = await _userManager.GetUserAsync(principal);

        if (user == null)
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(user, "Super Admin"))
        {
            return true;
        }

        if (!await _userManager.IsInRoleAsync(
                user, "Pitt Meadows Admin"))
        {
            return false;
        }

        return user.MunicipalityId == municipalityId;
    }

    public bool CanManageAllMunicipalities(
        ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true
            && principal.IsInRole("Super Admin");
    }
}