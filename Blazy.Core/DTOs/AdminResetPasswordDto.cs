using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for admin resetting a user's password
/// </summary>
public class AdminResetPasswordDto
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the new password.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}