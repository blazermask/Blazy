namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for reports
/// </summary>
public class ReportDto
{
    public int Id { get; set; }
    public int ReporterId { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // "Post", "Comment", "Account"
    public int? TargetPostId { get; set; }
    public string? TargetPostTitle { get; set; }
    public int? TargetCommentId { get; set; }
    public string? TargetCommentContent { get; set; }
    public int? TargetUserId { get; set; }
    public string? TargetUsername { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsReviewed { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public string? ReviewedByAdminUsername { get; set; }
    public string? Status { get; set; } // "Pending", "Resolved", "Dismissed"
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}