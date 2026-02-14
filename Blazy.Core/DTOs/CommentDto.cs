namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for comment information
/// </summary>
public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
}