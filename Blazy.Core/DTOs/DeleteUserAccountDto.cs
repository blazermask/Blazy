using System.ComponentModel.DataAnnotations;

namespace Blazy.Core.DTOs;

/// <summary>
/// Data transfer object for deleting a user account by admin
/// </summary>
public class DeleteUserAccountDto
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Please provide a reason for deleting this account.")]
    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}