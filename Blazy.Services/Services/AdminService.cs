using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Repository.Interfaces;
using Blazy.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Services.Services;

/// <summary>
/// Service implementation for admin operations
/// </summary>
public class AdminService : Interfaces.IAdminService
{
    private readonly Blazy.Data.BlazyDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IReportService _reportService;

    public AdminService(
        Blazy.Data.BlazyDbContext context,
        IUserRepository userRepository,
        IPostRepository postRepository,
        IReportService reportService)
    {
        _context = context;
        _userRepository = userRepository;
        _postRepository = postRepository;
        _reportService = reportService;
    }

    public async Task<(bool Success, string Message)> DeletePostAsync(int adminId, DeletePostDto model)
    {
        var post = await _postRepository.GetByIdAsync(model.PostId);
        if (post == null)
        {
            return (false, "Post not found.");
        }

        if (post.IsDeleted)
        {
            return (false, "Post is already deleted.");
        }

        // Get the next deletion number for the user
        var maxDeletionNumber = await _context.Posts
            .Where(p => p.UserId == post.UserId && p.DeletionNumber.HasValue)
            .MaxAsync(p => (int?)p.DeletionNumber) ?? 0;

        post.IsDeleted = true;
        post.DeletionReason = model.Reason;
        post.DeletedByAdminId = adminId;
        post.DeletionNumber = maxDeletionNumber + 1;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);

        // Create audit log
        await CreateAuditLogAsync(adminId, "DeletePost", post.Id, null, model.Reason);

        return (true, "Post deleted successfully.");
    }

    public async Task<(bool Success, string Message)> BanUserAsync(int adminId, BanUserDto model)
    {
        var user = await _userRepository.GetByIdAsync(model.UserId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        if (user.IsBanned)
        {
            return (false, "User is already banned.");
        }

        user.IsBanned = true;
        user.IsPermanentlyBanned = model.IsPermanent;
        user.BanReason = model.Reason;
        user.BanUntilDate = model.IsPermanent ? null : model.BanUntilDate;
        user.BannedByAdminId = adminId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Create audit log
        await CreateAuditLogAsync(adminId, "BanUser", null, user.Id, model.Reason);

        return (true, $"User {(model.IsPermanent ? "permanently" : "temporarily")} banned successfully.");
    }

    public async Task<(bool Success, string Message)> UnbanUserAsync(int adminId, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        if (!user.IsBanned)
        {
            return (false, "User is not banned.");
        }

        user.IsBanned = false;
        user.IsPermanentlyBanned = false;
        user.BanReason = null;
        user.BanUntilDate = null;
        user.BannedByAdminId = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Create audit log
        await CreateAuditLogAsync(adminId, "UnbanUser", null, user.Id, "Ban lifted by admin");

        return (true, "User unbanned successfully.");
    }

    public async Task<(IEnumerable<AdminAuditLogDto> Logs, int TotalCount)> GetAuditLogsAsync(int pageIndex, int pageSize)
    {
        var query = _context.AdminAuditLogs
            .Include(log => log.Admin)
            .Include(log => log.TargetPost)
            .Include(log => log.TargetUser)
            .OrderByDescending(log => log.CreatedAt);

        var totalCount = await query.CountAsync();

        var logs = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var logDtos = logs.Select(log => new AdminAuditLogDto
        {
            Id = log.Id,
            AdminId = log.AdminId,
            AdminUsername = log.Admin?.Username ?? "Unknown",
            ActionType = log.ActionType,
            TargetPostId = log.TargetPostId,
            TargetPostTitle = log.TargetPost?.Title,
            TargetUserId = log.TargetUserId,
            TargetUsername = log.TargetUser?.Username,
            Reason = log.Reason,
            CreatedAt = log.CreatedAt
        });

        return (logDtos, totalCount);
    }

    public async Task<IEnumerable<AdminAuditLogDto>> GetAuditLogsByAdminAsync(int adminId)
    {
        var logs = await _context.AdminAuditLogs
            .Include(log => log.Admin)
            .Include(log => log.TargetPost)
            .Include(log => log.TargetUser)
            .Where(log => log.AdminId == adminId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();

        return logs.Select(log => new AdminAuditLogDto
        {
            Id = log.Id,
            AdminId = log.AdminId,
            AdminUsername = log.Admin?.Username ?? "Unknown",
            ActionType = log.ActionType,
            TargetPostId = log.TargetPostId,
            TargetPostTitle = log.TargetPost?.Title,
            TargetUserId = log.TargetUserId,
            TargetUsername = log.TargetUser?.Username,
            Reason = log.Reason,
            CreatedAt = log.CreatedAt
        });
    }

    public async Task<IEnumerable<AdminAuditLogDto>> GetAuditLogsByTargetUserAsync(int userId)
    {
        var logs = await _context.AdminAuditLogs
            .Include(log => log.Admin)
            .Include(log => log.TargetPost)
            .Include(log => log.TargetUser)
            .Where(log => log.TargetUserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();

        return logs.Select(log => new AdminAuditLogDto
        {
            Id = log.Id,
            AdminId = log.AdminId,
            AdminUsername = log.Admin?.Username ?? "Unknown",
            ActionType = log.ActionType,
            TargetPostId = log.TargetPostId,
            TargetPostTitle = log.TargetPost?.Title,
            TargetUserId = log.TargetUserId,
            TargetUsername = log.TargetUser?.Username,
            Reason = log.Reason,
            CreatedAt = log.CreatedAt
        });
    }

    private async Task CreateAuditLogAsync(int adminId, string actionType, int? targetPostId, int? targetUserId, string? reason)
    {
        var auditLog = new AdminAuditLog
        {
            AdminId = adminId,
            ActionType = actionType,
            TargetPostId = targetPostId,
            TargetUserId = targetUserId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> ReviewReportAsync(int adminId, int reportId, string status, string? adminNotes)
    {
        return await _reportService.ReviewReportAsync(adminId, reportId, status, adminNotes);
    }

    public async Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize)
    {
        return await _reportService.GetReportsAsync(pageIndex, pageSize);
    }

    public async Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetPendingReportsAsync(int pageIndex, int pageSize)
    {
        return await _reportService.GetPendingReportsAsync(pageIndex, pageSize);
    }
}