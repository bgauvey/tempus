using System.ComponentModel.DataAnnotations;

namespace Tempus.Core.Models;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int Capacity { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Building { get; set; }

    [MaxLength(50)]
    public string? Floor { get; set; }

    /// <summary>
    /// JSON array of amenities (e.g., ["Projector", "Whiteboard", "Video Conference"])
    /// </summary>
    public string? Amenities { get; set; }

    public bool IsAvailable { get; set; } = true;

    [MaxLength(100)]
    public string? ImageUrl { get; set; }

    // Owner/Organization
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<RoomBooking> Bookings { get; set; } = new List<RoomBooking>();

    // Helper methods
    public List<string> GetAmenities()
    {
        if (string.IsNullOrWhiteSpace(Amenities))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Amenities) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public void SetAmenities(List<string> amenities)
    {
        Amenities = System.Text.Json.JsonSerializer.Serialize(amenities);
    }
}
