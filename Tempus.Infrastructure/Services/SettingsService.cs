using Microsoft.EntityFrameworkCore;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly TempusDbContext _context;

    public SettingsService(TempusDbContext context)
    {
        _context = context;
    }

    public async Task<CalendarSettings?> GetUserSettingsAsync(string userId)
    {
        return await _context.CalendarSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<CalendarSettings> CreateOrUpdateSettingsAsync(CalendarSettings settings)
    {
        var existingSettings = await GetUserSettingsAsync(settings.UserId);

        if (existingSettings != null)
        {
            // Update existing settings
            existingSettings.StartOfWeek = settings.StartOfWeek;
            existingSettings.TimeFormat = settings.TimeFormat;
            existingSettings.DateFormat = settings.DateFormat;
            existingSettings.ShowWeekNumbers = settings.ShowWeekNumbers;
            existingSettings.TimeZone = settings.TimeZone;
            existingSettings.DefaultCalendarView = settings.DefaultCalendarView;
            existingSettings.ShowWeekendInWeekView = settings.ShowWeekendInWeekView;
            existingSettings.TimeSlotDuration = settings.TimeSlotDuration;
            existingSettings.ScrollToTime = settings.ScrollToTime;
            existingSettings.WorkHoursStart = settings.WorkHoursStart;
            existingSettings.WorkHoursEnd = settings.WorkHoursEnd;
            existingSettings.WeekendDays = settings.WeekendDays;
            existingSettings.WorkingDays = settings.WorkingDays;
            existingSettings.LunchBreakStart = settings.LunchBreakStart;
            existingSettings.LunchBreakEnd = settings.LunchBreakEnd;
            existingSettings.BufferTimeBetweenEvents = settings.BufferTimeBetweenEvents;
            existingSettings.DefaultMeetingDuration = settings.DefaultMeetingDuration;
            existingSettings.DefaultEventColor = settings.DefaultEventColor;
            existingSettings.DefaultEventVisibility = settings.DefaultEventVisibility;
            existingSettings.DefaultLocation = settings.DefaultLocation;
            existingSettings.EmailNotificationsEnabled = settings.EmailNotificationsEnabled;
            existingSettings.DesktopNotificationsEnabled = settings.DesktopNotificationsEnabled;
            existingSettings.DefaultReminderTimes = settings.DefaultReminderTimes;
            existingSettings.DefaultCalendarId = settings.DefaultCalendarId;
            existingSettings.UpdatedAt = DateTime.UtcNow;

            _context.CalendarSettings.Update(existingSettings);
            await _context.SaveChangesAsync();
            return existingSettings;
        }
        else
        {
            // Create new settings
            _context.CalendarSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }
    }

    public async Task<CalendarSettings> GetOrCreateDefaultSettingsAsync(string userId)
    {
        var settings = await GetUserSettingsAsync(userId);

        if (settings == null)
        {
            // Create default settings
            settings = new CalendarSettings
            {
                UserId = userId
                // All other properties will use their default values from the model
            };

            _context.CalendarSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }
}
