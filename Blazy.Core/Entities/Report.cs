using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazy.Core.Entities;

/// <summary>
/// Represents a report made by a user against content (post, comment, or account)
/// </summary>
public class Report
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReporterId { get; set; }

    /// <summary>
    /// Type of content being reported: "Post", "Comment", or "Account"
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string ContentType { get; set; } = string.Empty;

    public int? TargetPostId { get; set; }

    public int? TargetCommentId { get; set; }

    public int? TargetUserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Whether the report has been reviewed by an admin
    /// </summary>
    public bool IsReviewed { get; set; } = false;

    /// <summary>
    /// ID of the admin who reviewed this report
    /// </summary>
    public int? ReviewedByAdminId { get; set; }

    /// <summary>
    /// Action taken on the report: "Pending", "Resolved", "Dismissed"
    /// </summary>
    [MaxLength(20)]
    public string? Status { get; set; }

    /// <summary>
    /// Notes from the admin who reviewed the report
    /// </summary>
    [MaxLength(500)]
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the report was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    [ForeignKey("ReporterId")]
    public virtual User Reporter { get; set; } = null!;

    [ForeignKey("ReviewedByAdminId")]
    public virtual User? ReviewedByAdmin { get; set; }

    [ForeignKey("TargetPostId")]
    public virtual Post? TargetPost { get; set; }

    [ForeignKey("TargetCommentId")]
    public virtual Comment? TargetComment { get; set; }

    [ForeignKey("TargetUserId")]
    public virtual User? TargetUser { get; set; }
}