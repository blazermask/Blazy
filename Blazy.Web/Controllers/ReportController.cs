using Blazy.Core.DTOs;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazy.Web.Controllers;

/// <summary>
/// Controller for reporting content (posts, comments, users)
/// </summary>
[Authorize]
public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly IUserService _userService;

    public ReportController(IReportService reportService, IUserService userService)
    {
        _reportService = reportService;
        _userService = userService;
    }

    /// <summary>
    /// Show report form for a post
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreatePostReport(int postId)
    {
        ViewBag.PostId = postId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePostReport(CreateReportDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.ContentType = "Post";
        model.TargetPostId = int.Parse(Request.Form["postId"]);
        model.TargetCommentId = null;
        model.TargetUserId = null;

        var reporterId = GetCurrentUserId();
        var result = await _reportService.CreateReportAsync(reporterId, model);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Index", "Post", new { id = model.TargetPostId });
    }

    /// <summary>
    /// Show report form for a comment
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateCommentReport(int commentId, int postId)
    {
        ViewBag.CommentId = commentId;
        ViewBag.PostId = postId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCommentReport(CreateReportDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.ContentType = "Comment";
        model.TargetPostId = null;
        model.TargetCommentId = int.Parse(Request.Form["commentId"]);
        model.TargetUserId = null;

        var reporterId = GetCurrentUserId();
        var result = await _reportService.CreateReportAsync(reporterId, model);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Index", "Post", new { id = Request.Form["postId"] });
    }

    /// <summary>
    /// Show report form for a user account
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateUserReport(int userId, string username)
    {
        ViewBag.UserId = userId;
        ViewBag.Username = username;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUserReport(CreateReportDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.ContentType = "Account";
        model.TargetPostId = null;
        model.TargetCommentId = null;
        model.TargetUserId = int.Parse(Request.Form["userId"]);

        var reporterId = GetCurrentUserId();
        var result = await _reportService.CreateReportAsync(reporterId, model);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        var user = await _userService.GetUserByIdAsync(int.Parse(Request.Form["userId"]));
        TempData["Success"] = result.Message;
        return RedirectToAction("Index", "Blog", new { username = user?.Username });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}