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
    private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;

    public AdminService(
        Blazy.Data.BlazyDbContext context,
        IUserRepository userRepository,
        IPostRepository postRepository,
        IReportService reportService,
        Microsoft.AspNetCore.Identity.UserManager<User> userManager)
    {
        _context = context;
        _userRepository = userRepository;
        _postRepository = postRepository;
        _reportService = reportService;
        _userManager = userManager;
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

    public async Task<(bool Success, string Message)> DeleteUserAccountAsync(int adminId, int userId, string reason)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        // Check if trying to delete the original admin
        if (await IsOriginalAdminAsync(userId))
        {
            return (false, "Cannot delete the original admin account.");
        }

        // Check if trying to delete self
        if (adminId == userId)
        {
            return (false, "You cannot delete your own account.");
        }

        // Check if the target user is an admin
        var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isTargetAdmin)
        {
            // Only the original admin can delete other admins
            if (!await IsOriginalAdminAsync(adminId))
            {
                return (false, "Only the original admin account can delete other admin accounts.");
            }
        }

        // Delete all posts by the user
        await _postRepository.DeletePostsByUserAsync(userId);

        // Delete all comments by the user
        var userComments = await _postRepository.GetCommentsByUserAsync(userId);
        foreach (var comment in userComments)
        {
            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
        }
        await _postRepository.SaveChangesAsync();

        // Mark user as deleted
        user.IsDeleted = true;
        user.DeletedUsername = user.UserName;
        user.UserName = $"deleted_user_{userId}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        user.Email = $"deleted_{userId}@deleted.com";
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Create audit log
        await CreateAuditLogAsync(adminId, "DeleteUserAccount", null, userId, reason);

        return (true, "User account deleted successfully. All their posts have been removed.");
    }

    public async Task<(bool Success, string Message)> AssignAdminRoleAsync(int adminId, int targetUserId)
    {
        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
        {
            return (false, "User not found.");
        }

        // Check if user is already an admin
        var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
        if (isAdmin)
        {
            return (false, "User is already an admin.");
        }

        // Assign admin role
        var result = await _userManager.AddToRoleAsync(targetUser, "Admin");
        if (!result.Succeeded)
        {
            return (false, "Failed to assign admin role.");
        }

        // Create audit log
        await CreateAuditLogAsync(adminId, "AssignAdminRole", null, targetUserId, $"Admin role assigned to {targetUser.UserName}");

        return (true, $"Admin role successfully assigned to {targetUser.UserName}.");
    }

    public async Task<(bool Success, string Message)> RevokeAdminRoleAsync(int adminId, int targetUserId)
    {
        // Check if trying to revoke from original admin
        if (await IsOriginalAdminAsync(targetUserId))
        {
            return (false, "Cannot revoke admin role from the original admin.");
        }

        // Check if trying to revoke from self
        if (adminId == targetUserId)
        {
            return (false, "You cannot revoke your own admin role.");
        }

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
        {
            return (false, "User not found.");
        }

        // Check if user is an admin
        var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
        if (!isAdmin)
        {
            return (false, "User is not an admin.");
        }

        // Revoke admin role
        var result = await _userManager.RemoveFromRoleAsync(targetUser, "Admin");
        if (!result.Succeeded)
        {
            return (false, "Failed to revoke admin role.");
        }

        // Create audit log
        await CreateAuditLogAsync(adminId, "RevokeAdminRole", null, targetUserId, $"Admin role revoked from {targetUser.UserName}");

        return (true, $"Admin role successfully revoked from {targetUser.UserName}.");
    }

    public async Task<IEnumerable<UserDto>> GetAllAdminsAsync()
    {
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        
        return adminUsers.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Pronouns = user.Pronouns,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            IsDeleted = user.IsDeleted,
            IsBanned = user.IsBanned
        }).OrderBy(u => u.Id);
    }

    public async Task<bool> IsOriginalAdminAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // The original admin is the one with username "admin"
        return user.UserName?.ToLower() == "admin";
    }

    public async Task<(bool Success, string Message)> ResetUserPasswordAsync(int adminId, int targetUserId, string newPassword)
    {
        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
        {
            return (false, "User not found.");
        }

        // Admins cannot reset other admins' passwords - they must use Change Password for their own account
        var isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
        if (isTargetAdmin)
        {
            return (false, "Cannot reset an admin's password. Admins must use the Change Password feature for their own account.");
        }

        // Reset the password using UserManager
        var token = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
        var result = await _userManager.ResetPasswordAsync(targetUser, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Failed to reset password: {errors}");
        }

        targetUser.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(targetUser);

        // Create audit log
        await CreateAuditLogAsync(adminId, "ResetUserPassword", null, targetUserId, $"Password reset for user {targetUser.UserName}");

        return (true, $"Password for {targetUser.UserName} has been reset successfully.");
    }
}