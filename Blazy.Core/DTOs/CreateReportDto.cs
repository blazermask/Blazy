using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for creating a report
/// </summary>
public class CreateReportDto
{
    [Required]
    public string ContentType { get; set; } = string.Empty; // "Post", "Comment", "Account"

    public int? TargetPostId { get; set; }

    public int? TargetCommentId { get; set; }

    public int? TargetUserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}