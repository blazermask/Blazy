using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazy.Core.Entities;

/// <summary>
/// Represents a user in the Blazy social media platform
/// </summary>
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(50)]
    public string? Pronouns { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    /// <summary>
    /// Custom HTML content for user's profile page
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? CustomHtml { get; set; }

    /// <summary>
    /// Background image URL for profile
    /// </summary>
    [MaxLength(500)]
    public string? BackgroundUrl { get; set; }

    /// <summary>
    /// Banner image URL
    /// </summary>
    [MaxLength(500)]
    public string? BannerUrl { get; set; }

    /// <summary>
    /// Music URL (background music for profile)
    /// </summary>
    [MaxLength(500)]
    public string? MusicUrl { get; set; }

    /// <summary>
    /// Custom font family
    /// </summary>
    [MaxLength(100)]
    public string? CustomFont { get; set; }

    /// <summary>
    /// Custom CSS for profile styling
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? CustomCss { get; set; }

    [MaxLength(20)]
    public string? AccentColor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    public virtual ICollection<Dislike> Dislikes { get; set; } = new List<Dislike>();
    public virtual ICollection<Subscription> Subscribers { get; set; } = new List<Subscription>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<UserTag> Tags { get; set; } = new List<UserTag>();
}