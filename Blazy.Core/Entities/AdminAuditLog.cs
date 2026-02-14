using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazy.Core.Entities;

/// <summary>
/// Represents an audit log entry for admin actions
/// </summary>
public class AdminAuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AdminId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty; // "DeletePost", "BanUser", "UnbanUser", etc.

    public int? TargetPostId { get; set; }

    public int? TargetUserId { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("AdminId")]
    public virtual User Admin { get; set; } = null!;

    [ForeignKey("TargetPostId")]
    public virtual Post? TargetPost { get; set; }

    [ForeignKey("TargetUserId")]
    public virtual User? TargetUser { get; set; }
}