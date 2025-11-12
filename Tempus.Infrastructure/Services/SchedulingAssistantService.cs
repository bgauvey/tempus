using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class SchedulingAssistantService : ISchedulingAssistantService
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<SchedulingAssistantService> _logger;

    // Default working hours (can be made configurable)
    private readonly TimeSpan _workDayStart = new(9, 0, 0);
    private readonly TimeSpan _workDayEnd = new(17, 0, 0);

    public SchedulingAssistantService(
        IDbContextFactory<TempusDbContext> contextFactory,
        ILogger<SchedulingAssistantService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<SchedulingSuggestion>> FindOptimalTimesAsync(
        List<string> attendeeEmails,
        int durationMinutes,
        DateTime searchStartDate,
        DateTime searchEndDate,
        int maxSuggestions = 5)
    {
        _logger.LogInformation(
            "Finding optimal times for {Count} attendees, duration {Duration}min, from {Start} to {End}",
            attendeeEmails.Count, durationMinutes, searchStartDate, searchEndDate);

        var suggestions = new List<SchedulingSuggestion>();
        var currentSlot = GetNextWorkingHourSlot(searchStartDate);

        while (currentSlot < searchEndDate && suggestions.Count < maxSuggestions * 3)
        {
            var endSlot = currentSlot.AddMinutes(durationMinutes);

            // Skip if outside working hours or spans multiple days
            if (!IsWithinWorkingHours(currentSlot, endSlot) || currentSlot.Date != endSlot.Date)
            {
                currentSlot = GetNextWorkingHourSlot(currentSlot.AddMinutes(30));
                continue;
            }

            var availability = await AnalyzeAvailabilityAsync(attendeeEmails, currentSlot, endSlot);

            var suggestion = new SchedulingSuggestion
            {
                StartTime = currentSlot,
                EndTime = endSlot,
                AvailableCount = availability.AvailableAttendees,
                TotalCount = availability.TotalAttendees,
                ConflictCount = availability.BusyAttendees,
                AvailableAttendees = availability.AvailableEmails,
                ConflictingAttendees = availability.BusyEmails,
                ConflictingEvents = availability.ConflictingEvents,
                Score = CalculateSuggestionScore(availability, currentSlot)
            };

            suggestion.ReasonForSuggestion = GenerateReasonForSuggestion(suggestion, currentSlot);

            suggestions.Add(suggestion);
            currentSlot = currentSlot.AddMinutes(30); // Move to next 30-min slot
        }

        // Rank suggestions by score and return top results
        var rankedSuggestions = suggestions
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.AvailableCount)
            .Take(maxSuggestions)
            .ToList();

        for (int i = 0; i < rankedSuggestions.Count; i++)
        {
            rankedSuggestions[i].Rank = i + 1;
        }

        _logger.LogInformation("Found {Count} optimal time suggestions", rankedSuggestions.Count);
        return rankedSuggestions;
    }

    public async Task<AvailabilitySlot> AnalyzeAvailabilityAsync(
        List<string> attendeeEmails,
        DateTime startTime,
        DateTime endTime)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var slot = new AvailabilitySlot
        {
            StartTime = startTime,
            EndTime = endTime,
            TotalAttendees = attendeeEmails.Count
        };

        // Get all events that overlap with this time slot for these attendees
        var conflictingEvents = await context.Events
            .Include(e => e.Attendees)
            .Where(e => e.StartTime < endTime && e.EndTime > startTime)
            .Where(e => e.Attendees.Any(a => attendeeEmails.Contains(a.Email)))
            .ToListAsync();

        var busyEmails = conflictingEvents
            .SelectMany(e => e.Attendees)
            .Where(a => attendeeEmails.Contains(a.Email))
            .Select(a => a.Email)
            .Distinct()
            .ToList();

        slot.BusyEmails = busyEmails;
        slot.BusyAttendees = busyEmails.Count;
        slot.AvailableEmails = attendeeEmails.Except(busyEmails).ToList();
        slot.AvailableAttendees = slot.AvailableEmails.Count;
        slot.ConflictingEvents = conflictingEvents;

        return slot;
    }

    public async Task<List<AvailabilitySlot>> GetAvailabilityGridAsync(
        List<string> attendeeEmails,
        DateTime startDate,
        DateTime endDate,
        int slotDurationMinutes = 30)
    {
        var slots = new List<AvailabilitySlot>();
        var currentSlot = GetNextWorkingHourSlot(startDate);

        while (currentSlot < endDate)
        {
            var endSlot = currentSlot.AddMinutes(slotDurationMinutes);

            // Only include slots within working hours
            if (IsWithinWorkingHours(currentSlot, endSlot) && currentSlot.Date == endSlot.Date)
            {
                var availability = await AnalyzeAvailabilityAsync(attendeeEmails, currentSlot, endSlot);
                slots.Add(availability);
            }

            currentSlot = currentSlot.AddMinutes(slotDurationMinutes);

            // Skip to next working day if we've passed working hours
            if (currentSlot.TimeOfDay > _workDayEnd)
            {
                currentSlot = GetNextWorkingHourSlot(currentSlot.Date.AddDays(1));
            }
        }

        return slots;
    }

    public async Task<SchedulingSuggestion?> FindNextAvailableSlotAsync(
        List<string> attendeeEmails,
        int durationMinutes,
        DateTime? startSearchFrom = null)
    {
        var searchStart = startSearchFrom ?? DateTime.Now;
        var searchEnd = searchStart.AddDays(14); // Search up to 2 weeks ahead

        var suggestions = await FindOptimalTimesAsync(
            attendeeEmails,
            durationMinutes,
            searchStart,
            searchEnd,
            maxSuggestions: 20);

        return suggestions.FirstOrDefault(s => s.AllAvailable);
    }

    public async Task<List<Event>> DetectConflictsAsync(
        List<string> attendeeEmails,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeEventId = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Events
            .Include(e => e.Attendees)
            .Where(e => e.StartTime < endTime && e.EndTime > startTime)
            .Where(e => e.Attendees.Any(a => attendeeEmails.Contains(a.Email)));

        if (excludeEventId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEventId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<List<SchedulingSuggestion>> SuggestAlternativeTimesAsync(
        Guid existingEventId,
        int maxSuggestions = 3)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var existingEvent = await context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == existingEventId);

        if (existingEvent == null)
        {
            _logger.LogWarning("Event {EventId} not found for alternative time suggestions", existingEventId);
            return new List<SchedulingSuggestion>();
        }

        var attendeeEmails = existingEvent.Attendees.Select(a => a.Email).ToList();
        var duration = (int)(existingEvent.EndTime - existingEvent.StartTime).TotalMinutes;

        // Search for alternatives in the same week
        var searchStart = existingEvent.StartTime.Date;
        var searchEnd = searchStart.AddDays(7);

        return await FindOptimalTimesAsync(attendeeEmails, duration, searchStart, searchEnd, maxSuggestions);
    }

    public async Task<bool> IsTimeAvailableForAllAsync(
        List<string> attendeeEmails,
        DateTime startTime,
        DateTime endTime)
    {
        var availability = await AnalyzeAvailabilityAsync(attendeeEmails, startTime, endTime);
        return availability.AllAvailable;
    }

    #region Helper Methods

    private DateTime GetNextWorkingHourSlot(DateTime fromTime)
    {
        var date = fromTime.Date;
        var time = fromTime.TimeOfDay;

        // If it's a weekend, move to Monday
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
            time = _workDayStart;
        }

        // If before work hours, set to start of work day
        if (time < _workDayStart)
        {
            return date.Add(_workDayStart);
        }

        // If after work hours, move to next work day
        if (time >= _workDayEnd)
        {
            date = date.AddDays(1);
            return GetNextWorkingHourSlot(date.Add(_workDayStart));
        }

        return date.Add(time);
    }

    private bool IsWithinWorkingHours(DateTime start, DateTime end)
    {
        // Skip weekends
        if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
            return false;

        var startTime = start.TimeOfDay;
        var endTime = end.TimeOfDay;

        return startTime >= _workDayStart && endTime <= _workDayEnd;
    }

    private double CalculateSuggestionScore(AvailabilitySlot availability, DateTime startTime)
    {
        double score = availability.AvailabilityPercentage;

        // Bonus for all attendees available
        if (availability.AllAvailable)
        {
            score += 10;
        }

        // Preference for mid-morning or mid-afternoon times
        var hour = startTime.Hour;
        if (hour >= 10 && hour < 12) // Late morning
        {
            score += 5;
        }
        else if (hour >= 14 && hour < 16) // Mid afternoon
        {
            score += 3;
        }
        else if (hour < 9 || hour >= 16) // Early morning or late afternoon
        {
            score -= 5;
        }

        // Slight preference for Tuesday-Thursday
        if (startTime.DayOfWeek >= DayOfWeek.Tuesday && startTime.DayOfWeek <= DayOfWeek.Thursday)
        {
            score += 2;
        }

        // Penalty for Monday mornings
        if (startTime.DayOfWeek == DayOfWeek.Monday && hour < 11)
        {
            score -= 3;
        }

        // Penalty for Friday afternoons
        if (startTime.DayOfWeek == DayOfWeek.Friday && hour >= 15)
        {
            score -= 3;
        }

        return Math.Max(0, Math.Min(100, score));
    }

    private string GenerateReasonForSuggestion(SchedulingSuggestion suggestion, DateTime time)
    {
        if (suggestion.AllAvailable)
        {
            var dayName = time.DayOfWeek.ToString();
            var timeStr = time.ToString("h:mm tt");
            return $"Perfect time! All attendees available on {dayName} at {timeStr}.";
        }

        if (suggestion.Score >= 80)
        {
            return $"Excellent option with {suggestion.AvailableCount}/{suggestion.TotalCount} attendees available.";
        }

        if (suggestion.Score >= 60)
        {
            return $"Good option with most attendees available.";
        }

        return $"Alternative option with {suggestion.AvailableCount}/{suggestion.TotalCount} available.";
    }

    #endregion
}
