using System.ComponentModel.DataAnnotations;
using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class ResourceReservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Resource
    public Guid ResourceId { get; set; }
    public Resource? Resource { get; set; }

    // Event
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public int QuantityReserved { get; set; } = 1;

    public RoomBookingStatus Status { get; set; } = RoomBookingStatus.Confirmed;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
