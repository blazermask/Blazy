using Blazy.Core.DTOs;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazy.Web.Controllers;

/// <summary>
/// Blog controller handling user profile/blog pages
/// </summary>
public class BlogController : Controller
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;

    public BlogController(IUserService userService, IPostService postService)
    {
        _userService = userService;
        _postService = postService;
    }

    /// <summary>
    /// Displays a user's blog/profile page
    /// </summary>
    public async Task<IActionResult> Index(string username, int page = 1)
    {
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Index", "Home");
        }

        const int pageSize = 10;
        int? currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;

        var user = await _userService.GetUserByUsernameAsync(username, currentUserId);
        if (user == null)
        {
            return NotFound();
        }

        var (posts, totalCount) = await _postService.GetPostsByUserAsync(user.Id, page, pageSize, currentUserId);

        ViewBag.User = user;
        ViewBag.PageCount = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.TotalCount = totalCount;

        return View(posts);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userService.SubscribeAsync(currentUserId, userId);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }

        var user = await _userService.GetUserByIdAsync(userId);
        return RedirectToAction(nameof(Index), new { username = user?.Username });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userService.UnsubscribeAsync(currentUserId, userId);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }

        var user = await _userService.GetUserByIdAsync(userId);
        return RedirectToAction(nameof(Index), new { username = user?.Username });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}