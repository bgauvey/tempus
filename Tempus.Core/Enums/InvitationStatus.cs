namespace Tempus.Core.Enums;

public enum InvitationStatus
{
    Pending = 0,   // Invitation sent, awaiting response
    Accepted = 1,  // Invitation accepted, user joined team
    Declined = 2,  // Invitation declined by recipient
    Expired = 3    // Invitation expired (past expiration date)
}
