using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazy.Core.Entities;

/// <summary>
/// Tracks login attempts for IP-based lockout functionality
/// </summary>
public class LoginAttempt
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// IP address from which the login attempt was made
    /// </summary>
    [MaxLength(45)]
    [Required]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Username that was attempted (if provided)
    /// </summary>
    [MaxLength(50)]
    public string? AttemptedUsername { get; set; }

    /// <summary>
    /// Whether this login attempt was successful
    /// </summary>
    public bool WasSuccessful { get; set; }

    /// <summary>
    /// When the login attempt occurred
    /// </summary>
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}