using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MEMS.Data.Entities;
using MEMS.Shared.Dtos;

namespace MEMS.Admin.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginRequestDto model, [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= "/";

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Username and password are required.";
            return Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Invalid username or password.";
            return Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            TempData["ErrorMessage"] = "This account is temporarily locked due to failed attempts. Please try again later.";
            return Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        TempData["ErrorMessage"] = "Invalid username or password.";
        return Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }
}
