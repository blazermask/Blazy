using Blazy.Core.DTOs;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blazy.Web.Controllers;

/// <summary>
/// Search controller for finding users
/// </summary>
public class SearchController : Controller
{
    private readonly IUserService _userService;

    public SearchController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Search page with results
    /// </summary>
    public async Task<IActionResult> Index(string q, int page = 1)
    {
        const int pageSize = 20;
        int? currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;

        if (!string.IsNullOrEmpty(q))
        {
            var (users, totalCount) = await _userService.SearchUsersAsync(q, page, pageSize, currentUserId);
            ViewBag.Users = users;
            ViewBag.TotalCount = totalCount;
            ViewBag.SearchQuery = q;
            ViewBag.PageCount = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentPage = page;
        }
        else
        {
            ViewBag.Users = Enumerable.Empty<UserDto>();
            ViewBag.TotalCount = 0;
            ViewBag.SearchQuery = "";
            ViewBag.PageCount = 0;
            ViewBag.CurrentPage = 1;
        }

        return View();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}