using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazy.Core.Entities;

/// <summary>
/// Represents a subscription relationship between users
/// Many-to-many relationship through this junction entity
/// </summary>
public class Subscription
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SubscriberId { get; set; }

    [Required]
    public int SubscribedToId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("SubscriberId")]
    public virtual User Subscriber { get; set; } = null!;

    [ForeignKey("SubscribedToId")]
    public virtual User SubscribedTo { get; set; } = null!;
}