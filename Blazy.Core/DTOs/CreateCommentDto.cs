using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for creating a new comment
/// </summary>
public class CreateCommentDto
{
    [Required(ErrorMessage = "Comment content is required")]
    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
    public string Content { get; set; } = string.Empty;
}