using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blazy.Web.Controllers;

/// <summary>
/// Account controller handling user registration, login, and profile management
/// </summary>
public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;

    public AccountController(IUserService userService, UserManager<User> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _userService.LoginAsync(model, ipAddress);
        if (result.Success)
        {
            // Sign in the user with role claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.User!.Username),
                new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString())
            };

            // Add role claims so User.IsInRole() works
            var user = await _userManager.FindByNameAsync(result.User.Username);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Identity.Application");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _userService.RegisterAsync(model, ipAddress);
        if (result.Success)
        {
            // Auto-login after registration with role claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.User!.Username),
                new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString())
            };

            // Add role claims
            var user = await _userManager.FindByNameAsync(result.User.Username);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Identity.Application");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };

            await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Identity.Application");
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        var result = await _userService.UpdateProfileAsync(
            userId,
            model.FirstName,
            model.LastName,
            model.Pronouns,
            model.Bio,
            model.CustomHtml,
            model.BackgroundUrl,
            model.BannerUrl,
            model.MusicUrl,
            model.CustomFont,
            model.CustomCss,
            model.AccentColor);

        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    [Authorize]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(Blazy.Core.DTOs.ChangePasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        var result = await _userService.ChangePasswordAsync(userId, model);

        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(DeleteAccountDto model)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.DeleteAccountAsync(userId, model);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            var user = await _userService.GetUserByIdAsync(userId);
            return View(user);
        }

        await HttpContext.SignOutAsync("Identity.Application");
        return RedirectToAction("Index", "Home");
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}