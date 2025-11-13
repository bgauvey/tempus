namespace Tempus.Core.Models;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Ownership and metadata
    public string CreatedBy { get; set; } = string.Empty; // User ID
    public ApplicationUser? Creator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public List<TeamMember> Members { get; set; } = new();
    public List<TeamInvitation> Invitations { get; set; } = new();

    // Helper methods
    public int GetMemberCount()
    {
        return Members.Count;
    }

    public bool IsUserOwner(string userId)
    {
        return Members.Any(m => m.UserId == userId && m.Role == Enums.TeamRole.Owner);
    }

    public bool IsUserAdmin(string userId)
    {
        return Members.Any(m => m.UserId == userId &&
            (m.Role == Enums.TeamRole.Owner || m.Role == Enums.TeamRole.Admin));
    }

    public bool IsUserMember(string userId)
    {
        return Members.Any(m => m.UserId == userId);
    }
}
