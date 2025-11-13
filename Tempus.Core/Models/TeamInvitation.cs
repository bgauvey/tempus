using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class TeamInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Team relationship
    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    // Invitation details
    public string Email { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string Token { get; set; } = string.Empty; // Unique token for invitation link

    // Who sent the invitation
    public string InvitedBy { get; set; } = string.Empty; // User ID
    public ApplicationUser? Inviter { get; set; }

    // Timestamps
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7); // Default 7 days
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }

    // Role to be assigned when invitation is accepted
    public TeamRole Role { get; set; } = TeamRole.Member;

    // Helper methods
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }

    public bool IsPending()
    {
        return Status == InvitationStatus.Pending && !IsExpired();
    }

    public bool CanBeAccepted()
    {
        return Status == InvitationStatus.Pending && !IsExpired();
    }

    public string GetStatusDisplayName()
    {
        if (Status == InvitationStatus.Pending && IsExpired())
        {
            return "Expired";
        }

        return Status switch
        {
            InvitationStatus.Pending => "Pending",
            InvitationStatus.Accepted => "Accepted",
            InvitationStatus.Declined => "Declined",
            InvitationStatus.Expired => "Expired",
            _ => "Unknown"
        };
    }

    public static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
