using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;

namespace Tempus.Infrastructure.Services;

/// <summary>
/// Service for generating calendar analytics and insights
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IEventRepository _eventRepository;

    public AnalyticsService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<CalendarAnalytics> GenerateAnalyticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var analytics = new CalendarAnalytics
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };

        // Get all events in the date range
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);

        // Calculate metrics
        analytics.TotalEvents = events.Count;
        analytics.MeetingCount = events.Count(e => e.EventType == EventType.Meeting);
        analytics.TotalHours = events.Sum(e => (int)(e.EndTime - e.StartTime).TotalHours);
        analytics.TotalMeetingCost = events.Where(e => e.EventType == EventType.Meeting).Sum(e => e.MeetingCost ?? 0);

        // Generate detailed metrics
        analytics.TimeUsage = await GetTimeUsageMetricsAsync(userId, startDate, endDate);
        analytics.MeetingStats = await GetMeetingAnalyticsAsync(userId, startDate, endDate);
        analytics.Productivity = await GetProductivityMetricsAsync(userId, startDate, endDate);

        return analytics;
    }

    public async Task<CalendarAnalytics> GetQuickStatsAsync(string userId, int days = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        return await GenerateAnalyticsAsync(userId, startDate, endDate);
    }

    public async Task<double> CalculateCalendarHealthScoreAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);

        double score = 100.0;

        // Calculate total days and hours
        var totalDays = (endDate - startDate).Days + 1;
        var totalAvailableHours = totalDays * 12; // Assuming 12 productive hours per day
        var totalScheduledHours = events.Sum(e => (e.EndTime - e.StartTime).TotalHours);

        // Factor 1: Scheduled vs Free Time (ideal is 60-70%)
        var scheduledPercentage = (totalScheduledHours / totalAvailableHours) * 100;
        if (scheduledPercentage > 80)
            score -= (scheduledPercentage - 80) * 0.5; // Penalty for over-scheduling
        else if (scheduledPercentage < 40)
            score -= (40 - scheduledPercentage) * 0.3; // Slight penalty for under-scheduling

        // Factor 2: Back-to-back meetings (should be minimized)
        var backToBackCount = CountBackToBackMeetings(events);
        score -= backToBackCount * 2; // -2 points per back-to-back meeting

        // Factor 3: Meeting distribution (avoid clustering)
        var meetingsByDay = events
            .Where(e => e.EventType == EventType.Meeting)
            .GroupBy(e => e.StartTime.Date)
            .Select(g => g.Count())
            .ToList();

        var daysWithTooManyMeetings = meetingsByDay.Count(count => count > 5);
        score -= daysWithTooManyMeetings * 5; // -5 points per overloaded day

        // Factor 4: Focus time availability
        var focusTimeBlocks = events.Count(e => e.EventType == EventType.TimeBlock &&
                                                (e.EndTime - e.StartTime).TotalMinutes >= 90);
        score += Math.Min(focusTimeBlocks * 2, 20); // +2 points per focus block, max +20

        // Ensure score is between 0 and 100
        return Math.Max(0, Math.Min(100, score));
    }

    public async Task<TimeUsageMetrics> GetTimeUsageMetricsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);
        var metrics = new TimeUsageMetrics();

        // Event type breakdown
        foreach (var eventType in Enum.GetValues<EventType>())
        {
            var typeEvents = events.Where(e => e.EventType == eventType).ToList();
            var hours = (int)typeEvents.Sum(e => (e.EndTime - e.StartTime).TotalHours);
            var count = typeEvents.Count;

            if (count > 0)
            {
                metrics.EventTypeHours[eventType.ToString()] = hours;
                metrics.EventTypeCounts[eventType.ToString()] = count;
            }
        }

        // Day of week distribution
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var dayEvents = events.Where(e => e.StartTime.DayOfWeek == day);
            var hours = (int)dayEvents.Sum(e => (e.EndTime - e.StartTime).TotalHours);
            metrics.DayOfWeekHours[day] = hours;
        }

        // Hour of day distribution
        for (int hour = 0; hour < 24; hour++)
        {
            var hourEvents = events.Where(e => e.StartTime.Hour == hour);
            metrics.HourOfDayDistribution[hour] = hourEvents.Count();
        }

        // Calculate scheduled vs free time
        var totalDays = (endDate - startDate).Days + 1;
        var totalAvailableHours = totalDays * 12; // Assuming 12 productive hours per day
        metrics.TotalScheduledHours = (int)events.Sum(e => (e.EndTime - e.StartTime).TotalHours);
        metrics.TotalFreeHours = totalAvailableHours - metrics.TotalScheduledHours;
        metrics.ScheduledPercentage = (metrics.TotalScheduledHours / (double)totalAvailableHours) * 100;

        return metrics;
    }

    public async Task<MeetingAnalytics> GetMeetingAnalyticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);
        var meetings = events.Where(e => e.EventType == EventType.Meeting).ToList();

        var analytics = new MeetingAnalytics
        {
            TotalMeetings = meetings.Count,
            TotalMeetingHours = (int)meetings.Sum(e => (e.EndTime - e.StartTime).TotalHours),
            TotalMeetingCost = meetings.Sum(e => e.MeetingCost ?? 0)
        };

        if (meetings.Any())
        {
            analytics.AverageMeetingDuration = meetings.Average(e => (e.EndTime - e.StartTime).TotalMinutes);
            analytics.AverageMeetingCost = analytics.TotalMeetingCost / meetings.Count;
        }

        // Meetings by day
        analytics.MeetingsByDay = meetings
            .GroupBy(e => e.StartTime.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Meetings by hour
        analytics.MeetingsByHour = meetings
            .GroupBy(e => e.StartTime.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        // Top attendees
        var attendeeStats = new Dictionary<string, (string Email, int Count, int Hours)>();
        foreach (var meeting in meetings)
        {
            foreach (var attendee in meeting.Attendees)
            {
                var key = $"{attendee.Name}_{attendee.Email}";
                if (attendeeStats.ContainsKey(key))
                {
                    var current = attendeeStats[key];
                    attendeeStats[key] = (current.Email, current.Count + 1,
                                         current.Hours + (int)(meeting.EndTime - meeting.StartTime).TotalHours);
                }
                else
                {
                    attendeeStats[key] = (attendee.Email, 1, (int)(meeting.EndTime - meeting.StartTime).TotalHours);
                }
            }
        }

        analytics.TopAttendees = attendeeStats
            .OrderByDescending(kvp => kvp.Value.Count)
            .Take(10)
            .Select(kvp => new TopAttendee
            {
                Name = kvp.Key.Split('_')[0],
                Email = kvp.Value.Email,
                MeetingCount = kvp.Value.Count,
                TotalHours = kvp.Value.Hours
            })
            .ToList();

        // Most costly meetings
        analytics.MostCostlyMeetings = meetings
            .Where(e => e.MeetingCost.HasValue)
            .OrderByDescending(e => e.MeetingCost)
            .Take(10)
            .Select(e => new CostlyMeeting
            {
                Title = e.Title,
                Date = e.StartTime,
                Cost = e.MeetingCost ?? 0,
                AttendeeCount = e.Attendees.Count,
                DurationMinutes = (int)(e.EndTime - e.StartTime).TotalMinutes
            })
            .ToList();

        // Back-to-back meetings
        analytics.BackToBackMeetings = CountBackToBackMeetings(meetings);
        analytics.MeetingsWithBreaks = meetings.Count - analytics.BackToBackMeetings;

        return analytics;
    }

    public async Task<ProductivityMetrics> GetProductivityMetricsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);
        var metrics = new ProductivityMetrics();

        // Calculate calendar health score
        metrics.CalendarHealthScore = await CalculateCalendarHealthScoreAsync(userId, startDate, endDate);

        // Focus time blocks (90+ minute uninterrupted blocks)
        var focusBlocks = events.Where(e => e.EventType == EventType.TimeBlock &&
                                           (e.EndTime - e.StartTime).TotalMinutes >= 90).ToList();
        metrics.FocusTimeBlocks = focusBlocks.Count;
        metrics.TotalFocusHours = (int)focusBlocks.Sum(e => (e.EndTime - e.StartTime).TotalHours);

        // Task completion metrics
        var tasks = events.Where(e => e.EventType == EventType.Task).ToList();
        var completedTasks = tasks.Count(t => t.EndTime < DateTime.Now);
        var overdueTasks = tasks.Count(t => t.EndTime < DateTime.Now && t.StartTime > DateTime.Now);

        metrics.CompletedTasks = completedTasks;
        metrics.OverdueTasks = overdueTasks;
        metrics.TaskCompletionRate = tasks.Any() ? (completedTasks / (double)tasks.Count) * 100 : 0;

        // Fragmented hours (hours with multiple short events)
        metrics.FragmentedHours = CalculateFragmentedHours(events);

        // Generate recommendations
        metrics.Recommendations = await GetRecommendationsAsync(userId, startDate, endDate);

        // Generate warnings
        metrics.Warnings = GenerateWarnings(events, metrics.CalendarHealthScore);

        return metrics;
    }

    public async Task<List<string>> GetRecommendationsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var recommendations = new List<string>();
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate, userId);
        var meetings = events.Where(e => e.EventType == EventType.Meeting).ToList();

        // Check for back-to-back meetings
        var backToBackCount = CountBackToBackMeetings(meetings);
        if (backToBackCount > 3)
        {
            recommendations.Add($"You have {backToBackCount} back-to-back meetings. Consider adding 15-minute breaks between meetings.");
        }

        // Check meeting costs
        var totalCost = meetings.Sum(e => e.MeetingCost ?? 0);
        if (totalCost > 10000)
        {
            recommendations.Add($"Meeting costs total ${totalCost:N0} for this period. Review if all meetings are necessary.");
        }

        // Check for focus time
        var focusBlocks = events.Count(e => e.EventType == EventType.TimeBlock &&
                                           (e.EndTime - e.StartTime).TotalMinutes >= 90);
        if (focusBlocks < 3)
        {
            recommendations.Add("Schedule more focus time blocks (90+ minutes) for deep work.");
        }

        // Check scheduling density
        var days = (endDate - startDate).Days + 1;
        var avgEventsPerDay = events.Count / (double)days;
        if (avgEventsPerDay > 8)
        {
            recommendations.Add("Your schedule is very dense. Consider consolidating or delegating some events.");
        }

        // Best time for deep work
        var hourDistribution = events.GroupBy(e => e.StartTime.Hour).ToDictionary(g => g.Key, g => g.Count());
        var leastBusyHours = hourDistribution.Where(kvp => kvp.Key >= 8 && kvp.Key <= 17)
                                            .OrderBy(kvp => kvp.Value)
                                            .Take(2)
                                            .Select(kvp => kvp.Key)
                                            .ToList();
        if (leastBusyHours.Any())
        {
            var timeStr = string.Join(" and ", leastBusyHours.Select(h => $"{h}:00"));
            recommendations.Add($"Your calendar shows the least activity at {timeStr}. This could be ideal for focused work.");
        }

        return recommendations;
    }

    // Helper methods

    private int CountBackToBackMeetings(List<Event> events)
    {
        var meetings = events.Where(e => e.EventType == EventType.Meeting)
                            .OrderBy(e => e.StartTime)
                            .ToList();

        int count = 0;
        for (int i = 0; i < meetings.Count - 1; i++)
        {
            var current = meetings[i];
            var next = meetings[i + 1];

            // Check if next meeting starts within 5 minutes of current ending
            if ((next.StartTime - current.EndTime).TotalMinutes <= 5)
            {
                count++;
            }
        }

        return count;
    }

    private int CalculateFragmentedHours(List<Event> events)
    {
        // Group events by date and hour
        var eventsByHour = events
            .GroupBy(e => new { e.StartTime.Date, e.StartTime.Hour })
            .Where(g => g.Count() > 2) // Hours with more than 2 events are fragmented
            .Count();

        return eventsByHour;
    }

    private List<string> GenerateWarnings(List<Event> events, double healthScore)
    {
        var warnings = new List<string>();

        // Calendar health warnings
        if (healthScore < 60)
        {
            warnings.Add("Calendar health score is below 60. Review your scheduling patterns.");
        }

        // Overload warnings
        var meetingsByDay = events
            .Where(e => e.EventType == EventType.Meeting)
            .GroupBy(e => e.StartTime.Date)
            .Where(g => g.Count() > 5)
            .ToList();

        if (meetingsByDay.Any())
        {
            warnings.Add($"{meetingsByDay.Count} day(s) have more than 5 meetings. Risk of burnout.");
        }

        // Long meeting warnings
        var longMeetings = events
            .Where(e => e.EventType == EventType.Meeting && (e.EndTime - e.StartTime).TotalHours > 2)
            .Count();

        if (longMeetings > 3)
        {
            warnings.Add($"{longMeetings} meetings exceed 2 hours. Consider breaking them into shorter sessions.");
        }

        return warnings;
    }
}
