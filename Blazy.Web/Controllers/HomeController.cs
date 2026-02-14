using Blazy.Core.DTOs;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazy.Web.Controllers;

/// <summary>
/// Home controller handling main page and navigation
/// </summary>
public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    public HomeController(IPostService postService, IUserService userService)
    {
        _postService = postService;
        _userService = userService;
    }

    /// <summary>
    /// Homepage with tabs for New, Loved, and Subscribed posts
    /// </summary>
    public async Task<IActionResult> Index(string tab = "new", int page = 1)
    {
        const int pageSize = 10;
        int? currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;

        IEnumerable<PostDto> posts;
        int totalCount = 0;

        switch (tab.ToLower())
        {
            case "loved":
                (posts, totalCount) = await _postService.GetMostLovedPostsAsync(page, pageSize, currentUserId);
                break;
            case "subscribed":
                if (currentUserId.HasValue)
                {
                    (posts, totalCount) = await _postService.GetSubscribedPostsAsync(currentUserId.Value, page, pageSize, currentUserId);
                }
                else
                {
                    (posts, totalCount) = await _postService.GetNewestPostsAsync(page, pageSize, currentUserId);
                }
                break;
            case "new":
            default:
                (posts, totalCount) = await _postService.GetNewestPostsAsync(page, pageSize, currentUserId);
                break;
        }

        ViewBag.CurrentTab = tab;
        ViewBag.PageCount = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.TotalCount = totalCount;

        return View(posts);
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    private int GetCurrentUserId()
    {
        if (User.Identity?.Name != null)
        {
            var user = _userService.GetUserByUsernameAsync(User.Identity.Name).Result;
            return user?.Id ?? 0;
        }
        return 0;
    }
}