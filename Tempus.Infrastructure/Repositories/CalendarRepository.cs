using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<CalendarRepository> _logger;

    public CalendarRepository(IDbContextFactory<TempusDbContext> contextFactory, ILogger<CalendarRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Calendar?> GetByIdAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Calendars
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<List<Calendar>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Calendars
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Calendar?> GetDefaultCalendarAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Calendars
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsDefault);
    }

    public async Task<Calendar> CreateAsync(Calendar calendar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync();
        return calendar;
    }

    public async Task<Calendar> UpdateAsync(Calendar calendar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Calendars.Update(calendar);
        calendar.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return calendar;
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var calendar = await context.Calendars
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (calendar != null)
        {
            // Don't delete if it's the only calendar
            var calendarCount = await context.Calendars
                .CountAsync(c => c.UserId == userId);

            if (calendarCount <= 1)
            {
                throw new InvalidOperationException("Cannot delete the last calendar. Users must have at least one calendar.");
            }

            // If deleting the default calendar, set another calendar as default
            if (calendar.IsDefault)
            {
                var newDefault = await context.Calendars
                    .Where(c => c.UserId == userId && c.Id != id)
                    .OrderBy(c => c.SortOrder)
                    .FirstOrDefaultAsync();

                if (newDefault != null)
                {
                    newDefault.IsDefault = true;
                    context.Calendars.Update(newDefault);
                }
            }

            context.Calendars.Remove(calendar);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetEventCountAsync(Guid calendarId, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Events
            .CountAsync(e => e.CalendarId == calendarId && e.UserId == userId);
    }

    public async Task<bool> SetDefaultCalendarAsync(Guid calendarId, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Verify the calendar exists and belongs to the user
        var calendar = await context.Calendars
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);

        if (calendar == null)
        {
            return false;
        }

        // Unset all other default calendars for this user
        var otherCalendars = await context.Calendars
            .Where(c => c.UserId == userId && c.Id != calendarId && c.IsDefault)
            .ToListAsync();

        foreach (var cal in otherCalendars)
        {
            cal.IsDefault = false;
        }

        // Set this calendar as default
        calendar.IsDefault = true;
        calendar.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<int> UpdateSortOrderAsync(List<(Guid Id, int SortOrder)> updates, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var calendarIds = updates.Select(u => u.Id).ToList();
        var calendars = await context.Calendars
            .Where(c => c.UserId == userId && calendarIds.Contains(c.Id))
            .ToListAsync();

        foreach (var calendar in calendars)
        {
            var update = updates.FirstOrDefault(u => u.Id == calendar.Id);
            if (update != default)
            {
                calendar.SortOrder = update.SortOrder;
                calendar.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await context.SaveChangesAsync();
    }

    public async Task<Calendar> EnsureDefaultCalendarExistsAsync(string userId, string userEmail)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if user has any calendars
        var existingCalendar = await context.Calendars
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (existingCalendar != null)
        {
            return existingCalendar;
        }

        // Create a default calendar for the user
        var defaultCalendar = new Calendar
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "My Calendar",
            Description = "Default calendar",
            Color = "#1E88E5", // Blue
            IsVisible = true,
            IsDefault = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };

        context.Calendars.Add(defaultCalendar);
        await context.SaveChangesAsync();

        _logger.LogDebug("Created default calendar for user {UserEmail}", userEmail);
        return defaultCalendar;
    }
}
