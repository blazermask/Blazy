using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for banning a user
/// </summary>
public class BanUserDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public bool IsPermanent { get; set; } = false;

    public DateTime? BanUntilDate { get; set; }
}