using Blazy.Core.DTOs;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazy.Web.Controllers;

/// <summary>
/// Post controller handling post creation, viewing, and management
/// </summary>
public class PostController : Controller
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IUserService _userService;

    public PostController(IPostService postService, ICommentService commentService, IUserService userService)
    {
        _postService = postService;
        _commentService = commentService;
        _userService = userService;
    }

    /// <summary>
    /// View a single post with comments
    /// </summary>
    public async Task<IActionResult> Index(int id)
    {
        int? currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
        var post = await _postService.GetPostByIdAsync(id, currentUserId);
        
        if (post == null)
        {
            return NotFound();
        }

        var comments = await _commentService.GetCommentsByPostAsync(id);
        ViewBag.Comments = comments;

        return View(post);
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        var result = await _postService.CreatePostAsync(userId, model);

        if (result.Success)
        {
            return RedirectToAction(nameof(Index), new { id = result.Post?.Id });
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int id)
    {
        var userId = GetCurrentUserId();
        await _postService.LikePostAsync(id, userId);
        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlike(int id)
    {
        var userId = GetCurrentUserId();
        await _postService.UnlikePostAsync(id, userId);
        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dislike(int id)
    {
        var userId = GetCurrentUserId();
        await _postService.DislikePostAsync(id, userId);
        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Undislike(int id)
    {
        var userId = GetCurrentUserId();
        await _postService.UndislikePostAsync(id, userId);
        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(int id, CreateCommentDto model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index), new { id });
        }

        var userId = GetCurrentUserId();
        await _commentService.CreateCommentAsync(id, userId, model);

        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var isAdmin = await _userService.IsAdminAsync(userId);
        
        // Admins can delete any post, users can only delete their own
        if (isAdmin)
        {
            // For admin deletion, use the admin service or direct deletion
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                TempData["Error"] = "Post not found.";
                return RedirectToAction(nameof(Index), new { id });
            }
            
            var result = await _postService.DeletePostAsync(id, userId, true);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index), new { id });
            }
        }
        else
        {
            var result = await _postService.DeletePostByUserAsync(id, userId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index), new { id });
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id, int postId)
    {
        var userId = GetCurrentUserId();
        var result = await _commentService.DeleteCommentByUserAsync(id, userId);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { id = postId });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}