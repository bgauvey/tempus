using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class TeamMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Team relationship
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    // User relationship
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Member details
    public TeamRole Role { get; set; } = TeamRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Who invited this member
    public string? InvitedBy { get; set; } // User ID
    public ApplicationUser? Inviter { get; set; }

    // Helper methods
    public bool CanManageMembers()
    {
        return Role == TeamRole.Owner || Role == TeamRole.Admin;
    }

    public bool IsOwner()
    {
        return Role == TeamRole.Owner;
    }

    public string GetRoleDisplayName()
    {
        return Role switch
        {
            TeamRole.Owner => "Owner",
            TeamRole.Admin => "Admin",
            TeamRole.Member => "Member",
            _ => "Unknown"
        };
    }
}
