using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

/// <summary>
/// Service for handling meeting RSVP responses and invitations
/// </summary>
public class RSVPService : IRSVPService
{
    private readonly TempusDbContext _context;
    private readonly IEmailNotificationService _emailService;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<RSVPService> _logger;

    public RSVPService(
        TempusDbContext context,
        IEmailNotificationService emailService,
        INotificationRepository notificationRepository,
        ILogger<RSVPService> logger)
    {
        _context = context;
        _emailService = emailService;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Submit an RSVP response for an attendee
    /// </summary>
    public async Task<Attendee> SubmitResponseAsync(Guid eventId, string attendeeEmail, AttendeeStatus status, string? responseNotes = null)
    {
        var attendee = await _context.Attendees
            .Include(a => a.Event)
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.Email == attendeeEmail);

        if (attendee == null)
        {
            _logger.LogWarning("Attendee {Email} not found for event {EventId}", attendeeEmail, eventId);
            throw new InvalidOperationException($"Attendee {attendeeEmail} not found for this event");
        }

        var oldStatus = attendee.Status;
        attendee.Status = status;
        attendee.ResponseDate = DateTime.UtcNow;
        attendee.ResponseNotes = responseNotes;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Attendee {Email} changed RSVP status from {OldStatus} to {NewStatus} for event {EventId}",
            attendeeEmail, oldStatus, status, eventId);

        // Notify organizer about the response
        await NotifyOrganizerOfResponseAsync(attendee);

        return attendee;
    }

    /// <summary>
    /// Get RSVP statistics for an event
    /// </summary>
    public async Task<RSVPStatistics> GetRSVPStatisticsAsync(Guid eventId)
    {
        var attendees = await _context.Attendees
            .Where(a => a.EventId == eventId)
            .ToListAsync();

        var stats = new RSVPStatistics
        {
            TotalInvited = attendees.Count,
            Accepted = attendees.Count(a => a.Status == AttendeeStatus.Accepted),
            Declined = attendees.Count(a => a.Status == AttendeeStatus.Declined),
            Tentative = attendees.Count(a => a.Status == AttendeeStatus.Tentative),
            Pending = attendees.Count(a => a.Status == AttendeeStatus.Pending)
        };

        return stats;
    }

    /// <summary>
    /// Propose an alternative time for a meeting
    /// </summary>
    public async Task<ProposedTime> ProposeAlternativeTimeAsync(
        Guid eventId,
        string attendeeEmail,
        DateTime proposedStartTime,
        DateTime proposedEndTime,
        string? reason = null)
    {
        // Check if event allows proposed times
        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        if (!evt.AllowProposedTimes)
        {
            throw new InvalidOperationException("This event does not allow alternative time proposals");
        }

        var attendee = await _context.Attendees
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.Email == attendeeEmail);

        if (attendee == null)
        {
            throw new InvalidOperationException("Attendee not found for this event");
        }

        var proposedTime = new ProposedTime
        {
            AttendeeId = attendee.Id,
            ProposedStartTime = proposedStartTime,
            ProposedEndTime = proposedEndTime,
            Reason = reason,
            VoteCount = 1, // Proposer automatically votes for their own proposal
            VotedByEmails = new List<string> { attendeeEmail }
        };

        _context.ProposedTimes.Add(proposedTime);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Attendee {Email} proposed alternative time {StartTime} - {EndTime} for event {EventId}",
            attendeeEmail, proposedStartTime, proposedEndTime, eventId);

        // Notify organizer about the proposal
        await NotifyOrganizerOfProposalAsync(evt, attendee, proposedTime);

        return proposedTime;
    }

    /// <summary>
    /// Vote on a proposed alternative time
    /// </summary>
    public async Task<ProposedTime> VoteOnProposedTimeAsync(Guid proposedTimeId, string voterEmail)
    {
        var proposedTime = await _context.ProposedTimes
            .Include(p => p.Attendee)
                .ThenInclude(a => a!.Event)
            .FirstOrDefaultAsync(p => p.Id == proposedTimeId);

        if (proposedTime == null)
        {
            throw new InvalidOperationException("Proposed time not found");
        }

        // Check if voter is an attendee
        var voter = await _context.Attendees
            .FirstOrDefaultAsync(a => a.EventId == proposedTime.Attendee!.EventId && a.Email == voterEmail);

        if (voter == null)
        {
            throw new InvalidOperationException("Only attendees can vote on proposed times");
        }

        // Check if already voted
        if (proposedTime.VotedByEmails.Contains(voterEmail))
        {
            _logger.LogWarning("Attendee {Email} already voted for proposed time {ProposedTimeId}", voterEmail, proposedTimeId);
            return proposedTime;
        }

        proposedTime.VotedByEmails.Add(voterEmail);
        proposedTime.VoteCount++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Attendee {Email} voted for proposed time {ProposedTimeId}", voterEmail, proposedTimeId);

        return proposedTime;
    }

    /// <summary>
    /// Get all proposed times for an event
    /// </summary>
    public async Task<List<ProposedTime>> GetProposedTimesAsync(Guid eventId)
    {
        var proposedTimes = await _context.ProposedTimes
            .Include(p => p.Attendee)
            .Where(p => p.Attendee!.EventId == eventId)
            .OrderByDescending(p => p.VoteCount)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return proposedTimes;
    }

    /// <summary>
    /// Get attendees who haven't responded yet
    /// </summary>
    public async Task<List<Attendee>> GetNonRespondersAsync(Guid eventId)
    {
        var nonResponders = await _context.Attendees
            .Include(a => a.Event)
            .Where(a => a.EventId == eventId && a.Status == AttendeeStatus.Pending)
            .ToListAsync();

        return nonResponders;
    }

    /// <summary>
    /// Send RSVP reminder to non-responders
    /// </summary>
    public async Task SendRSVPRemindersAsync(Guid eventId)
    {
        var evt = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        if (!evt.SendReminderToNonResponders)
        {
            _logger.LogInformation("Event {EventId} has reminders disabled", eventId);
            return;
        }

        var nonResponders = evt.Attendees.Where(a => a.Status == AttendeeStatus.Pending).ToList();

        _logger.LogInformation("Sending RSVP reminders to {Count} non-responders for event {EventId}",
            nonResponders.Count, eventId);

        foreach (var attendee in nonResponders)
        {
            try
            {
                await _emailService.SendRSVPReminderAsync(evt, attendee);

                attendee.LastReminderSent = DateTime.UtcNow;
                attendee.ReminderCount++;

                // Create notification
                await _notificationRepository.CreateAsync(new Notification
                {
                    UserId = evt.UserId,
                    EventId = evt.Id,
                    Type = NotificationType.ReminderSent,
                    Title = "RSVP Reminder Sent",
                    Message = $"RSVP reminder sent to {attendee.Name} ({attendee.Email}) for {evt.Title}",
                    IsRead = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send RSVP reminder to {Email} for event {EventId}",
                    attendee.Email, eventId);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Check if an attendee can see the guest list based on visibility settings
    /// </summary>
    public async Task<bool> CanViewGuestListAsync(Guid eventId, string attendeeEmail)
    {
        var evt = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null)
        {
            return false;
        }

        var attendee = evt.Attendees.FirstOrDefault(a => a.Email == attendeeEmail);
        if (attendee == null)
        {
            return false;
        }

        return evt.GuestListVisibility switch
        {
            GuestListVisibility.AllAttendees => true,
            GuestListVisibility.OrganizerOnly => attendee.IsOrganizer,
            GuestListVisibility.Hidden => false,
            _ => false
        };
    }

    /// <summary>
    /// Get visible attendees based on guest list visibility settings
    /// </summary>
    public async Task<List<Attendee>> GetVisibleAttendeesAsync(Guid eventId, string requestingAttendeeEmail)
    {
        var evt = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null)
        {
            return new List<Attendee>();
        }

        var requestingAttendee = evt.Attendees.FirstOrDefault(a => a.Email == requestingAttendeeEmail);
        if (requestingAttendee == null)
        {
            return new List<Attendee>();
        }

        return evt.GuestListVisibility switch
        {
            GuestListVisibility.AllAttendees => evt.Attendees.ToList(),
            GuestListVisibility.OrganizerOnly => requestingAttendee.IsOrganizer
                ? evt.Attendees.ToList()
                : new List<Attendee> { requestingAttendee },
            GuestListVisibility.Hidden => new List<Attendee> { requestingAttendee },
            _ => new List<Attendee>()
        };
    }

    /// <summary>
    /// Update RSVP deadline for an event
    /// </summary>
    public async Task UpdateRSVPDeadlineAsync(Guid eventId, DateTime? deadline)
    {
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null)
        {
            throw new InvalidOperationException("Event not found");
        }

        evt.RSVPDeadline = deadline;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated RSVP deadline for event {EventId} to {Deadline}", eventId, deadline);
    }

    #region Private Helper Methods

    private async Task NotifyOrganizerOfResponseAsync(Attendee attendee)
    {
        if (attendee.Event == null)
        {
            return;
        }

        var organizer = await _context.Attendees
            .FirstOrDefaultAsync(a => a.EventId == attendee.EventId && a.IsOrganizer);

        if (organizer == null)
        {
            return;
        }

        try
        {
            await _emailService.SendRSVPResponseNotificationAsync(attendee.Event, attendee, organizer);

            // Create notification
            await _notificationRepository.CreateAsync(new Notification
            {
                UserId = attendee.Event.UserId,
                EventId = attendee.EventId,
                Type = NotificationType.RSVPResponse,
                Title = "RSVP Response Received",
                Message = $"{attendee.Name} has {attendee.Status.ToString().ToLower()} your invitation to {attendee.Event.Title}",
                IsRead = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify organizer of RSVP response for event {EventId}", attendee.EventId);
        }
    }

    private async Task NotifyOrganizerOfProposalAsync(Event evt, Attendee attendee, ProposedTime proposedTime)
    {
        var organizer = await _context.Attendees
            .FirstOrDefaultAsync(a => a.EventId == evt.Id && a.IsOrganizer);

        if (organizer == null)
        {
            return;
        }

        try
        {
            await _emailService.SendProposedTimeNotificationAsync(evt, attendee, proposedTime, organizer);

            // Create notification
            await _notificationRepository.CreateAsync(new Notification
            {
                UserId = evt.UserId,
                EventId = evt.Id,
                Type = NotificationType.ProposedTimeSubmitted,
                Title = "Alternative Time Proposed",
                Message = $"{attendee.Name} has proposed an alternative time for {evt.Title}",
                IsRead = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify organizer of proposed time for event {EventId}", evt.Id);
        }
    }

    #endregion
}
