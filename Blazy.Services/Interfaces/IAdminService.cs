using Blazy.Core.DTOs;

namespace Blazy.Services.Interfaces;

/// <summary>
/// Service interface for admin operations
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Delete a post with a reason
    /// </summary>
    Task<(bool Success, string Message)> DeletePostAsync(int adminId, DeletePostDto model);

    /// <summary>
    /// Ban a user (temporary or permanent)
    /// </summary>
    Task<(bool Success, string Message)> BanUserAsync(int adminId, BanUserDto model);

    /// <summary>
    /// Unban a user
    /// </summary>
    Task<(bool Success, string Message)> UnbanUserAsync(int adminId, int userId);

    /// <summary>
    /// Get all audit logs
    /// </summary>
    Task<(IEnumerable<AdminAuditLogDto> Logs, int TotalCount)> GetAuditLogsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Get audit logs for a specific admin
    /// </summary>
    Task<IEnumerable<AdminAuditLogDto>> GetAuditLogsByAdminAsync(int adminId);

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    Task<IEnumerable<AdminAuditLogDto>> GetAuditLogsByTargetUserAsync(int userId);

    /// <summary>
    /// Review a report
    /// </summary>
    Task<(bool Success, string Message)> ReviewReportAsync(int adminId, int reportId, string status, string? adminNotes);

    /// <summary>
    /// Get all reports
    /// </summary>
    Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetReportsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Get pending reports
    /// </summary>
    Task<(IEnumerable<ReportDto> Reports, int TotalCount)> GetPendingReportsAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Delete a user account (admin only)
    /// </summary>
    Task<(bool Success, string Message)> DeleteUserAccountAsync(int adminId, int userId, string reason);

    /// <summary>
    /// Assign admin role to a user
    /// </summary>
    Task<(bool Success, string Message)> AssignAdminRoleAsync(int adminId, int targetUserId);

    /// <summary>
    /// Revoke admin role from a user (cannot revoke from original admin)
    /// </summary>
    Task<(bool Success, string Message)> RevokeAdminRoleAsync(int adminId, int targetUserId);

    /// <summary>
    /// Get all users with admin role
    /// </summary>
    Task<IEnumerable<UserDto>> GetAllAdminsAsync();

    /// <summary>
    /// Check if a user is the original admin (cannot be demoted)
    /// </summary>
    Task<bool> IsOriginalAdminAsync(int userId);
}