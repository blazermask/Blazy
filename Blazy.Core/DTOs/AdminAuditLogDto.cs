namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for admin audit logs
/// </summary>
public class AdminAuditLogDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminUsername { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public int? TargetPostId { get; set; }
    public string? TargetPostTitle { get; set; }
    public int? TargetUserId { get; set; }
    public string? TargetUsername { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}