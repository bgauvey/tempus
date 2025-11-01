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
        Console.WriteLine($"[SettingsService] CreateOrUpdateSettingsAsync called for userId: {settings.UserId}, DefaultCalendarView: {settings.DefaultCalendarView}");

        var existingSettings = await GetUserSettingsAsync(settings.UserId);

        if (existingSettings != null)
        {
            Console.WriteLine($"[SettingsService] Found existing settings. Current DefaultCalendarView: {existingSettings.DefaultCalendarView}");

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

            Console.WriteLine($"[SettingsService] Updating DefaultCalendarView to: {existingSettings.DefaultCalendarView}");

            _context.CalendarSettings.Update(existingSettings);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[SettingsService] Settings saved to database");

            return existingSettings;
        }
        else
        {
            Console.WriteLine($"[SettingsService] No existing settings found. Creating new settings.");

            // Create new settings
            _context.CalendarSettings.Add(settings);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[SettingsService] New settings created");

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
