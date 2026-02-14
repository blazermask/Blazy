using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for creating a new post
/// </summary>
public class CreatePostDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? MusicUrl { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
}