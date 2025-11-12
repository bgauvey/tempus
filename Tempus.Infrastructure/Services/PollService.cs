using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class PollService : IPollService
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<PollService> _logger;

    public PollService(
        IDbContextFactory<TempusDbContext> contextFactory,
        IEmailNotificationService emailService,
        ILogger<PollService> logger)
    {
        _contextFactory = contextFactory;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SchedulingPoll> CreatePollAsync(
        string title,
        string organizerEmail,
        string organizerName,
        List<DateTime> proposedStartTimes,
        int durationMinutes,
        string? description = null,
        string? location = null,
        DateTime? deadline = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = new SchedulingPoll
        {
            Title = title,
            Description = description,
            OrganizerEmail = organizerEmail,
            OrganizerName = organizerName,
            Location = location,
            Duration = durationMinutes,
            Deadline = deadline,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Create time slots
        foreach (var startTime in proposedStartTimes)
        {
            poll.TimeSlots.Add(new PollTimeSlot
            {
                StartTime = startTime,
                EndTime = startTime.AddMinutes(durationMinutes)
            });
        }

        context.SchedulingPolls.Add(poll);
        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Created poll {PollId} '{Title}' with {SlotCount} time slots",
            poll.Id, poll.Title, poll.TimeSlots.Count);

        return poll;
    }

    public async Task<SchedulingPoll?> GetPollByIdAsync(Guid pollId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SchedulingPolls
            .Include(p => p.TimeSlots)
                .ThenInclude(ts => ts.Responses)
            .Include(p => p.Responses)
            .FirstOrDefaultAsync(p => p.Id == pollId);
    }

    public async Task<List<SchedulingPoll>> GetPollsByOrganizerAsync(string organizerEmail)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .Include(p => p.Responses)
            .Where(p => p.OrganizerEmail == organizerEmail)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SchedulingPoll>> GetActivePollsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .Include(p => p.Responses)
            .Where(p => p.IsActive && !p.FinalizedAt.HasValue)
            .Where(p => !p.Deadline.HasValue || p.Deadline.Value > DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<SchedulingPoll> AddTimeSlotsAsync(Guid pollId, List<DateTime> startTimes)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new InvalidOperationException($"Poll {pollId} not found");

        if (poll.FinalizedAt.HasValue)
            throw new InvalidOperationException("Cannot add time slots to a finalized poll");

        foreach (var startTime in startTimes)
        {
            poll.TimeSlots.Add(new PollTimeSlot
            {
                StartTime = startTime,
                EndTime = startTime.AddMinutes(poll.Duration)
            });
        }

        await context.SaveChangesAsync();

        _logger.LogInformation("Added {Count} time slots to poll {PollId}", startTimes.Count, pollId);
        return poll;
    }

    public async Task<SchedulingPoll> RemoveTimeSlotAsync(Guid pollId, Guid timeSlotId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new InvalidOperationException($"Poll {pollId} not found");

        if (poll.FinalizedAt.HasValue)
            throw new InvalidOperationException("Cannot modify a finalized poll");

        var timeSlot = poll.TimeSlots.FirstOrDefault(ts => ts.Id == timeSlotId);
        if (timeSlot != null)
        {
            poll.TimeSlots.Remove(timeSlot);
            await context.SaveChangesAsync();
            _logger.LogInformation("Removed time slot {TimeSlotId} from poll {PollId}", timeSlotId, pollId);
        }

        return poll;
    }

    public async Task<PollResponse> SubmitResponseAsync(
        Guid pollId,
        Guid timeSlotId,
        string respondentEmail,
        string respondentName,
        PollResponseType response,
        string? comment = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new InvalidOperationException($"Poll {pollId} not found");

        if (!poll.IsActive)
            throw new InvalidOperationException("Poll is not active");

        if (poll.IsExpired())
            throw new InvalidOperationException("Poll deadline has passed");

        // Check if response already exists
        var existingResponse = await context.PollResponses
            .FirstOrDefaultAsync(r =>
                r.SchedulingPollId == pollId &&
                r.PollTimeSlotId == timeSlotId &&
                r.RespondentEmail == respondentEmail);

        if (existingResponse != null)
        {
            // Update existing response
            existingResponse.Response = response;
            existingResponse.Comment = comment;
            existingResponse.UpdatedAt = DateTime.UtcNow;
            await UpdateTimeSlotStatistics(context, timeSlotId);
            await context.SaveChangesAsync();
            return existingResponse;
        }

        // Create new response
        var pollResponse = new PollResponse
        {
            SchedulingPollId = pollId,
            PollTimeSlotId = timeSlotId,
            RespondentEmail = respondentEmail,
            RespondentName = respondentName,
            Response = response,
            Comment = comment,
            RespondedAt = DateTime.UtcNow
        };

        context.PollResponses.Add(pollResponse);
        await UpdateTimeSlotStatistics(context, timeSlotId);
        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Recorded {Response} response from {Email} for poll {PollId} time slot {TimeSlotId}",
            response, respondentEmail, pollId, timeSlotId);

        return pollResponse;
    }

    public async Task<List<PollResponse>> SubmitMultipleResponsesAsync(
        Guid pollId,
        string respondentEmail,
        string respondentName,
        Dictionary<Guid, PollResponseType> responses)
    {
        var submittedResponses = new List<PollResponse>();

        foreach (var (timeSlotId, responseType) in responses)
        {
            var response = await SubmitResponseAsync(
                pollId,
                timeSlotId,
                respondentEmail,
                respondentName,
                responseType);

            submittedResponses.Add(response);
        }

        return submittedResponses;
    }

    public async Task<PollResponse> UpdateResponseAsync(Guid responseId, PollResponseType newResponse, string? comment = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var response = await context.PollResponses.FindAsync(responseId);
        if (response == null)
            throw new InvalidOperationException($"Response {responseId} not found");

        response.Response = newResponse;
        response.Comment = comment;
        response.UpdatedAt = DateTime.UtcNow;

        await UpdateTimeSlotStatistics(context, response.PollTimeSlotId);
        await context.SaveChangesAsync();

        return response;
    }

    public async Task<List<PollResponse>> GetPollResponsesAsync(Guid pollId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.PollResponses
            .Where(r => r.SchedulingPollId == pollId)
            .OrderBy(r => r.RespondedAt)
            .ToListAsync();
    }

    public async Task<List<PollResponse>> GetTimeSlotResponsesAsync(Guid timeSlotId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.PollResponses
            .Where(r => r.PollTimeSlotId == timeSlotId)
            .OrderBy(r => r.RespondedAt)
            .ToListAsync();
    }

    public async Task<List<PollResponse>> GetUserPollResponsesAsync(Guid pollId, string userEmail)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.PollResponses
            .Where(r => r.SchedulingPollId == pollId && r.RespondentEmail == userEmail)
            .ToListAsync();
    }

    public async Task<SchedulingPoll> FinalizePollAsync(Guid pollId, Guid selectedTimeSlotId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new InvalidOperationException($"Poll {pollId} not found");

        if (poll.FinalizedAt.HasValue)
            throw new InvalidOperationException("Poll is already finalized");

        var timeSlot = poll.TimeSlots.FirstOrDefault(ts => ts.Id == selectedTimeSlotId);
        if (timeSlot == null)
            throw new InvalidOperationException($"Time slot {selectedTimeSlotId} not found in poll");

        poll.SelectedTimeSlotId = selectedTimeSlotId;
        poll.FinalizedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("Finalized poll {PollId} with time slot {TimeSlotId}", pollId, selectedTimeSlotId);
        return poll;
    }

    public async Task<SchedulingPoll> CancelPollAsync(Guid pollId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = await context.SchedulingPolls.FindAsync(pollId);
        if (poll == null)
            throw new InvalidOperationException($"Poll {pollId} not found");

        poll.IsActive = false;
        await context.SaveChangesAsync();

        _logger.LogInformation("Cancelled poll {PollId}", pollId);
        return poll;
    }

    public async Task<SchedulingPoll> GetPollResultsAsync(Guid pollId)
    {
        return await GetPollByIdAsync(pollId) ??
            throw new InvalidOperationException($"Poll {pollId} not found");
    }

    public async Task<PollTimeSlot?> GetMostPopularTimeSlotAsync(Guid pollId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.PollTimeSlots
            .Where(ts => ts.SchedulingPollId == pollId)
            .OrderByDescending(ts => ts.YesCount)
            .ThenByDescending(ts => ts.MaybeCount)
            .FirstOrDefaultAsync();
    }

    public async Task SendPollInvitationsAsync(Guid pollId, List<string> attendeeEmails)
    {
        var poll = await GetPollByIdAsync(pollId);
        if (poll == null)
        {
            _logger.LogWarning("Cannot send invitations for non-existent poll {PollId}", pollId);
            return;
        }

        foreach (var email in attendeeEmails)
        {
            await _emailService.SendPollInvitationAsync(poll, email);
        }

        _logger.LogInformation("Sent poll invitations for {PollId} to {Count} attendees", pollId, attendeeEmails.Count);
    }

    public async Task SendPollRemindersAsync(Guid pollId, List<string> attendeeEmails)
    {
        var poll = await GetPollByIdAsync(pollId);
        if (poll == null)
        {
            _logger.LogWarning("Cannot send reminders for non-existent poll {PollId}", pollId);
            return;
        }

        foreach (var email in attendeeEmails)
        {
            await _emailService.SendPollReminderAsync(poll, email);
        }

        _logger.LogInformation("Sent poll reminders for {PollId} to {Count} attendees", pollId, attendeeEmails.Count);
    }

    public async Task<Event> CreateEventFromPollAsync(Guid pollId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var poll = await context.SchedulingPolls
            .Include(p => p.TimeSlots)
            .Include(p => p.Responses)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new InvalidOperationException($"Poll {pollId} not found");

        if (!poll.SelectedTimeSlotId.HasValue)
            throw new InvalidOperationException("Poll must be finalized before creating an event");

        var selectedSlot = poll.TimeSlots.FirstOrDefault(ts => ts.Id == poll.SelectedTimeSlotId.Value);
        if (selectedSlot == null)
            throw new InvalidOperationException("Selected time slot not found");

        var newEvent = new Event
        {
            Title = poll.Title,
            Description = poll.Description,
            Location = poll.Location,
            StartTime = selectedSlot.StartTime,
            EndTime = selectedSlot.EndTime,
            UserId = poll.OrganizerEmail,
            CreatedAt = DateTime.UtcNow
        };

        // Add all poll respondents as attendees
        var respondentEmails = poll.Responses
            .Select(r => r.RespondentEmail)
            .Distinct()
            .ToList();

        foreach (var email in respondentEmails)
        {
            var response = poll.Responses.FirstOrDefault(r => r.RespondentEmail == email);
            newEvent.Attendees.Add(new Attendee
            {
                Email = email,
                Name = response?.RespondentName ?? email,
                Status = AttendeeStatus.Accepted
            });
        }

        context.Events.Add(newEvent);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created event {EventId} from poll {PollId}", newEvent.Id, pollId);
        return newEvent;
    }

    #region Helper Methods

    private async Task UpdateTimeSlotStatistics(TempusDbContext context, Guid timeSlotId)
    {
        var responses = await context.PollResponses
            .Where(r => r.PollTimeSlotId == timeSlotId)
            .ToListAsync();

        var timeSlot = await context.PollTimeSlots.FindAsync(timeSlotId);
        if (timeSlot == null) return;

        timeSlot.ResponseCount = responses.Count;
        timeSlot.YesCount = responses.Count(r => r.Response == PollResponseType.Yes);
        timeSlot.NoCount = responses.Count(r => r.Response == PollResponseType.No);
        timeSlot.MaybeCount = responses.Count(r => r.Response == PollResponseType.Maybe);
    }

    #endregion
}
