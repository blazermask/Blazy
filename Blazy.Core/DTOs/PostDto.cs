namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for post information
/// </summary>
public class PostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? MusicUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}