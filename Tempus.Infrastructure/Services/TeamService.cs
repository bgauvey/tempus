using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tempus.Core.Configuration;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly TempusDbContext _context;
    private readonly ILogger<TeamService> _logger;
    private readonly IEmailNotificationService _emailService;
    private readonly ApplicationSettings _appSettings;

    public TeamService(
        TempusDbContext context,
        ILogger<TeamService> logger,
        IEmailNotificationService emailService,
        IOptions<ApplicationSettings> appSettings)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _appSettings = appSettings.Value;
    }

    #region Team Management

    public async Task<Team> CreateTeamAsync(string name, string? description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Team name is required", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("Creator user ID is required", nameof(createdBy));
        }

        var team = new Team
        {
            Name = name,
            Description = description,
            CreatedBy = createdBy
        };

        // Add creator as owner
        var ownerMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = createdBy,
            Role = TeamRole.Owner
        };

        team.Members.Add(ownerMember);

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created team {TeamId} '{TeamName}' by user {UserId}",
            team.Id, team.Name, createdBy);

        return team;
    }

    public async Task<Team?> GetTeamByIdAsync(Guid teamId)
    {
        return await _context.Teams
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Invitations)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }

    public async Task<List<Team>> GetUserTeamsAsync(string userId)
    {
        return await _context.Teams
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Where(t => t.Members.Any(m => m.UserId == userId))
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Team> UpdateTeamAsync(Guid teamId, string name, string? description, string userId)
    {
        var team = await GetTeamByIdAsync(teamId);

        if (team == null)
        {
            throw new KeyNotFoundException($"Team {teamId} not found");
        }

        // Only team owners and admins can update team details
        if (!team.IsUserAdmin(userId))
        {
            throw new UnauthorizedAccessException("Only team owners and admins can update team details");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Team name is required", nameof(name));
        }

        team.Name = name;
        team.Description = description;
        team.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated team {TeamId} by user {UserId}", teamId, userId);

        return team;
    }

    public async Task<bool> DeleteTeamAsync(Guid teamId, string userId)
    {
        var team = await GetTeamByIdAsync(teamId);

        if (team == null)
        {
            return false;
        }

        // Only team owners can delete the team
        if (!team.IsUserOwner(userId))
        {
            throw new UnauthorizedAccessException("Only team owners can delete the team");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted team {TeamId} by user {UserId}", teamId, userId);

        return true;
    }

    #endregion

    #region Member Management

    public async Task<List<TeamMember>> GetTeamMembersAsync(Guid teamId)
    {
        return await _context.TeamMembers
            .Include(m => m.User)
            .Include(m => m.Inviter)
            .Where(m => m.TeamId == teamId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.JoinedAt)
            .ToListAsync();
    }

    public async Task<TeamMember?> GetTeamMemberAsync(Guid teamId, string userId)
    {
        return await _context.TeamMembers
            .Include(m => m.User)
            .Include(m => m.Team)
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
    }

    public async Task<bool> RemoveMemberAsync(Guid teamId, string userId, string removedBy)
    {
        var team = await GetTeamByIdAsync(teamId);

        if (team == null)
        {
            return false;
        }

        var member = await GetTeamMemberAsync(teamId, userId);

        if (member == null)
        {
            return false;
        }

        // Can't remove the owner
        if (member.Role == TeamRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove team owner. Transfer ownership first or delete the team.");
        }

        // Only team owners and admins can remove members
        if (!team.IsUserAdmin(removedBy))
        {
            throw new UnauthorizedAccessException("Only team owners and admins can remove members");
        }

        _context.TeamMembers.Remove(member);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed member {UserId} from team {TeamId} by {RemovedBy}",
            userId, teamId, removedBy);

        return true;
    }

    public async Task<bool> UpdateMemberRoleAsync(Guid teamId, string userId, TeamRole newRole, string updatedBy)
    {
        var team = await GetTeamByIdAsync(teamId);

        if (team == null)
        {
            return false;
        }

        var member = await GetTeamMemberAsync(teamId, userId);

        if (member == null)
        {
            return false;
        }

        // Only team owners can change roles
        if (!team.IsUserOwner(updatedBy))
        {
            throw new UnauthorizedAccessException("Only team owners can change member roles");
        }

        // Can't change the owner's role
        if (member.Role == TeamRole.Owner)
        {
            throw new InvalidOperationException("Cannot change the owner's role. Transfer ownership first.");
        }

        member.Role = newRole;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated member {UserId} role to {Role} in team {TeamId} by {UpdatedBy}",
            userId, newRole, teamId, updatedBy);

        return true;
    }

    #endregion

    #region Invitation Management

    public async Task<TeamInvitation> CreateInvitationAsync(Guid teamId, string email, TeamRole role, string invitedBy)
    {
        var team = await GetTeamByIdAsync(teamId);

        if (team == null)
        {
            throw new KeyNotFoundException($"Team {teamId} not found");
        }

        // Only team owners and admins can invite members
        if (!team.IsUserAdmin(invitedBy))
        {
            throw new UnauthorizedAccessException("Only team owners and admins can invite members");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required", nameof(email));
        }

        // Check if user is already a member
        var existingMember = await _context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.User!.Email == email);

        if (existingMember)
        {
            throw new InvalidOperationException("User is already a member of this team");
        }

        // Check if there's already a pending invitation
        var existingInvitation = await _context.TeamInvitations
            .FirstOrDefaultAsync(i => i.TeamId == teamId &&
                i.Email == email &&
                i.Status == InvitationStatus.Pending &&
                i.ExpiresAt > DateTime.UtcNow);

        if (existingInvitation != null)
        {
            throw new InvalidOperationException("There is already a pending invitation for this email");
        }

        var invitation = new TeamInvitation
        {
            TeamId = teamId,
            Email = email,
            Role = role,
            InvitedBy = invitedBy,
            Token = TeamInvitation.GenerateToken()
        };

        _context.TeamInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created invitation {InvitationId} for {Email} to team {TeamId} by {InvitedBy}",
            invitation.Id, email, teamId, invitedBy);

        // Send invitation email
        try
        {
            // Get the inviter's information
            var inviter = await _context.Users.FindAsync(invitedBy);
            var inviterName = inviter?.UserName ?? "A team member";
            var inviterEmail = inviter?.Email ?? "";

            var invitationUrl = GetInvitationUrl(invitation.Token);

            await _emailService.SendTeamInvitationAsync(
                teamName: team.Name,
                teamDescription: team.Description,
                inviteeEmail: email,
                inviterName: inviterName,
                inviterEmail: inviterEmail,
                invitationToken: invitation.Token,
                invitationUrl: invitationUrl,
                expiresAt: invitation.ExpiresAt,
                role: invitation.Role.ToString()
            );

            _logger.LogInformation("Sent invitation email to {Email} for team {TeamId}", email, teamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email for {InvitationId}. Invitation still created.", invitation.Id);
            // Don't fail the invitation creation if email fails
        }

        return invitation;
    }

    public async Task<List<TeamInvitation>> GetTeamInvitationsAsync(Guid teamId)
    {
        return await _context.TeamInvitations
            .Include(i => i.Inviter)
            .Include(i => i.Team)
            .Where(i => i.TeamId == teamId)
            .OrderByDescending(i => i.InvitedAt)
            .ToListAsync();
    }

    public async Task<List<TeamInvitation>> GetUserInvitationsAsync(string email)
    {
        return await _context.TeamInvitations
            .Include(i => i.Team)
            .Include(i => i.Inviter)
            .Where(i => i.Email == email && i.Status == InvitationStatus.Pending)
            .Where(i => i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.InvitedAt)
            .ToListAsync();
    }

    public async Task<TeamInvitation?> GetInvitationByTokenAsync(string token)
    {
        return await _context.TeamInvitations
            .Include(i => i.Team)
            .Include(i => i.Inviter)
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task<TeamMember> AcceptInvitationAsync(string token, string userId)
    {
        var invitation = await GetInvitationByTokenAsync(token);

        if (invitation == null)
        {
            throw new KeyNotFoundException("Invitation not found");
        }

        if (!invitation.CanBeAccepted())
        {
            throw new InvalidOperationException($"Invitation cannot be accepted. Status: {invitation.GetStatusDisplayName()}");
        }

        // Get user's email to verify it matches the invitation
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Email != invitation.Email)
        {
            throw new UnauthorizedAccessException("This invitation is for a different email address");
        }

        // Check if user is already a member
        var existingMember = await GetTeamMemberAsync(invitation.TeamId, userId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("You are already a member of this team");
        }

        // Create team member
        var member = new TeamMember
        {
            TeamId = invitation.TeamId,
            UserId = userId,
            Role = invitation.Role,
            InvitedBy = invitation.InvitedBy
        };

        _context.TeamMembers.Add(member);

        // Update invitation status
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} accepted invitation {InvitationId} to team {TeamId}",
            userId, invitation.Id, invitation.TeamId);

        return member;
    }

    public async Task<bool> DeclineInvitationAsync(string token)
    {
        var invitation = await GetInvitationByTokenAsync(token);

        if (invitation == null)
        {
            return false;
        }

        if (!invitation.CanBeAccepted())
        {
            throw new InvalidOperationException($"Invitation cannot be declined. Status: {invitation.GetStatusDisplayName()}");
        }

        invitation.Status = InvitationStatus.Declined;
        invitation.DeclinedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation {InvitationId} declined", invitation.Id);

        return true;
    }

    public async Task<bool> CancelInvitationAsync(Guid invitationId, string userId)
    {
        var invitation = await _context.TeamInvitations
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            return false;
        }

        // Only team owners and admins can cancel invitations
        if (invitation.Team != null && !invitation.Team.IsUserAdmin(userId))
        {
            throw new UnauthorizedAccessException("Only team owners and admins can cancel invitations");
        }

        _context.TeamInvitations.Remove(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation {InvitationId} cancelled by {UserId}", invitationId, userId);

        return true;
    }

    #endregion

    #region Authorization Helpers

    public async Task<bool> IsUserTeamOwnerAsync(Guid teamId, string userId)
    {
        return await _context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId && m.Role == TeamRole.Owner);
    }

    public async Task<bool> IsUserTeamAdminAsync(Guid teamId, string userId)
    {
        return await _context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId &&
                (m.Role == TeamRole.Owner || m.Role == TeamRole.Admin));
    }

    public async Task<bool> IsUserTeamMemberAsync(Guid teamId, string userId)
    {
        return await _context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId);
    }

    #endregion

    #region Private Helper Methods

    public string GetInvitationUrl(string token)
    {
        var baseUrl = _appSettings.BaseUrl.TrimEnd('/');
        return $"{baseUrl}/teams/accept-invitation?token={token}";
    }

    #endregion
}
