namespace Tempus.Core.Models;

public class CustomCalendarRange
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DaysCount { get; set; }
    public bool ShowWeekends { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // User ownership
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
