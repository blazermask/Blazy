using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.Entities;

/// <summary>
/// Tracks registration attempts by IP address to enforce daily limits
/// </summary>
public class RegistrationRecord
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// IP address of the registering client
    /// </summary>
    [Required]
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// When the registration occurred
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}