using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for managing teams and team memberships
/// </summary>
public interface ITeamService
{
    // Team management
    Task<Team> CreateTeamAsync(string name, string? description, string createdBy);
    Task<Team?> GetTeamByIdAsync(Guid teamId);
    Task<List<Team>> GetUserTeamsAsync(string userId);
    Task<Team> UpdateTeamAsync(Guid teamId, string name, string? description, string userId);
    Task<bool> DeleteTeamAsync(Guid teamId, string userId);

    // Member management
    Task<List<TeamMember>> GetTeamMembersAsync(Guid teamId);
    Task<TeamMember?> GetTeamMemberAsync(Guid teamId, string userId);
    Task<bool> RemoveMemberAsync(Guid teamId, string userId, string removedBy);
    Task<bool> UpdateMemberRoleAsync(Guid teamId, string userId, TeamRole newRole, string updatedBy);

    // Invitation management
    Task<TeamInvitation> CreateInvitationAsync(Guid teamId, string email, TeamRole role, string invitedBy);
    Task<List<TeamInvitation>> GetTeamInvitationsAsync(Guid teamId);
    Task<List<TeamInvitation>> GetUserInvitationsAsync(string email);
    Task<TeamInvitation?> GetInvitationByTokenAsync(string token);
    Task<TeamMember> AcceptInvitationAsync(string token, string userId);
    Task<bool> DeclineInvitationAsync(string token);
    Task<bool> CancelInvitationAsync(Guid invitationId, string userId);

    // Authorization helpers
    Task<bool> IsUserTeamOwnerAsync(Guid teamId, string userId);
    Task<bool> IsUserTeamAdminAsync(Guid teamId, string userId);
    Task<bool> IsUserTeamMemberAsync(Guid teamId, string userId);
}
