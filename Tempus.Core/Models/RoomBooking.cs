using System.ComponentModel.DataAnnotations;
using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class RoomBooking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Room
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }

    // Event
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public RoomBookingStatus Status { get; set; } = RoomBookingStatus.Confirmed;

    public DateTime? ConfirmationDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
