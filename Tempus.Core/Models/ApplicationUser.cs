using Microsoft.AspNetCore.Identity;

namespace Tempus.Core.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Calendar> Calendars { get; set; } = new List<Calendar>();
    public ICollection<CustomCalendarRange> CustomCalendarRanges { get; set; } = new List<CustomCalendarRange>();
}
