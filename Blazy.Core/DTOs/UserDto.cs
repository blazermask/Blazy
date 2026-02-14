namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Pronouns { get; set; }
    public string? Bio { get; set; }
    public string? CustomHtml { get; set; }
    public string? BackgroundUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? MusicUrl { get; set; }
    public string? CustomFont { get; set; }
    public string? CustomCss { get; set; }
    public string? AccentColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public int PostCount { get; set; }
    public int SubscriberCount { get; set; }
    public bool IsCurrentUser { get; set; }
    public bool IsSubscribed { get; set; }
    public bool IsBanned { get; set; }
    public bool IsPermanentlyBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanUntilDate { get; set; }
    public int? BannedByAdminId { get; set; }
    public string? BannedByAdminUsername { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsDeleted { get; set; }
}