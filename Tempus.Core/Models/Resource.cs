using System.ComponentModel.DataAnnotations;
using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ResourceType ResourceType { get; set; }

    /// <summary>
    /// Total quantity available
    /// </summary>
    public int Quantity { get; set; } = 1;

    [MaxLength(200)]
    public string? Location { get; set; }

    public bool IsAvailable { get; set; } = true;

    public ResourceCondition Condition { get; set; } = ResourceCondition.Good;

    [MaxLength(100)]
    public string? ImageUrl { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    // Owner/Organization
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<ResourceReservation> Reservations { get; set; } = new List<ResourceReservation>();
}
