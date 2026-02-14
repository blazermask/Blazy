using Blazy.Core.DTOs;
using Blazy.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazy.Web.Controllers;

/// <summary>
/// Admin controller for managing users, posts, and viewing audit logs
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;
    private readonly IPostService _postService;

    public AdminController(
        IAdminService adminService,
        IUserService userService,
        IPostService postService)
    {
        _adminService = adminService;
        _userService = userService;
        _postService = postService;
    }

    /// <summary>
    /// Admin dashboard
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Manage users page
    /// </summary>
    public async Task<IActionResult> Users(int page = 1, int pageSize = 20, string search = "")
    {
        if (string.IsNullOrEmpty(search))
        {
            // Get all users (excluding permanently banned)
            var allUsers = await _userService.GetAllUsersAsync(page, pageSize);
            return View(allUsers);
        }
        else
        {
            // Search users
            var searchResults = await _userService.SearchUsersAsync(search, page, pageSize);
            return View(searchResults);
        }
    }

    /// <summary>
    /// Ban user page
    /// </summary>
    public async Task<IActionResult> BanUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(BanUserDto model)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminService.BanUserAsync(adminId, model);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Users));
    }

    /// <summary>
    /// Unban user
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbanUser(int id)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminService.UnbanUserAsync(adminId, id);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    /// <summary>
    /// Manage posts page
    /// </summary>
    public IActionResult Posts(int page = 1, int pageSize = 20)
    {
        // Get all posts (including deleted ones for admin)
        var posts = _postService.GetAllPostsAsync(page, pageSize);
        return View(posts);
    }

    /// <summary>
    /// Delete post with reason page
    /// </summary>
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(DeletePostDto model)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminService.DeletePostAsync(adminId, model);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Posts));
    }

    /// <summary>
    /// Audit logs page
    /// </summary>
    public async Task<IActionResult> AuditLogs(int page = 1, int pageSize = 50)
    {
        var logs = await _adminService.GetAuditLogsAsync(page, pageSize);
        return View(logs);
    }

    /// <summary>
    /// Audit logs for specific admin
    /// </summary>
    public async Task<IActionResult> MyAuditLogs()
    {
        var adminId = GetCurrentUserId();
        var logs = await _adminService.GetAuditLogsByAdminAsync(adminId);
        return View(logs);
    }

    /// <summary>
    /// Audit logs for specific user
    /// </summary>
    public async Task<IActionResult> UserAuditLogs(int userId)
    {
        var logs = await _adminService.GetAuditLogsByTargetUserAsync(userId);
        var user = await _userService.GetUserByIdAsync(userId);
        ViewBag.Username = user?.Username ?? "Unknown";
        return View(logs);
    }

    /// <summary>
    /// Reports management page
    /// </summary>
    public async Task<IActionResult> Reports(int page = 1, int pageSize = 20, string status = "all")
    {
        var adminId = GetCurrentUserId();
        
        (IEnumerable<Blazy.Core.DTOs.ReportDto> Reports, int TotalCount) result;
        
        if (status == "pending")
        {
            result = await _adminService.GetPendingReportsAsync(page, pageSize);
        }
        else
        {
            result = await _adminService.GetReportsAsync(page, pageSize);
        }

        ViewBag.Reports = result.Reports;
        ViewBag.TotalCount = result.TotalCount;
        ViewBag.Status = status;
        ViewBag.PageCount = (int)Math.Ceiling((double)result.TotalCount / pageSize);
        ViewBag.CurrentPage = page;

        return View();
    }

    /// <summary>
    /// Review a report
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewReport(int reportId, string status, string? adminNotes)
    {
        var adminId = GetCurrentUserId();
        var result = await _adminService.ReviewReportAsync(adminId, reportId, status, adminNotes);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Reports));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}