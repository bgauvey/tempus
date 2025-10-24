namespace Tempus.Core.Models;

public class Contact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsAutoCreated { get; set; } = false; // True if created from meeting attendee

    // User ownership
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
