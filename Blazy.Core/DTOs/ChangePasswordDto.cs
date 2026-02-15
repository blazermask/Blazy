using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for changing user password
/// </summary>
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}