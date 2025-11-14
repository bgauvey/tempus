using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class CalendarSharingService : ICalendarSharingService
{
    private readonly TempusDbContext _context;
    private readonly ILogger<CalendarSharingService> _logger;
    private readonly IIcsImportService _icsImportService;

    public CalendarSharingService(
        TempusDbContext context,
        ILogger<CalendarSharingService> logger,
        IIcsImportService icsImportService)
    {
        _context = context;
        _logger = logger;
        _icsImportService = icsImportService;
    }

    #region Calendar Sharing

    public async Task<CalendarShare> ShareCalendarAsync(Guid calendarId, string sharedWithUserId,
        CalendarSharePermission permission, string sharedByUserId, string? note = null)
    {
        // Verify calendar exists and user has permission to share it
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == sharedByUserId);

        if (calendar == null)
        {
            throw new KeyNotFoundException($"Calendar {calendarId} not found or you don't have permission to share it");
        }

        // Check if already shared with this user
        var existingShare = await _context.CalendarShares
            .FirstOrDefaultAsync(s => s.CalendarId == calendarId && s.SharedWithUserId == sharedWithUserId);

        if (existingShare != null)
        {
            throw new InvalidOperationException("Calendar is already shared with this user");
        }

        // Verify the user being shared with exists
        var sharedWithUser = await _context.Users.FindAsync(sharedWithUserId);
        if (sharedWithUser == null)
        {
            throw new KeyNotFoundException($"User {sharedWithUserId} not found");
        }

        var share = new CalendarShare
        {
            CalendarId = calendarId,
            SharedWithUserId = sharedWithUserId,
            SharedByUserId = sharedByUserId,
            Permission = permission,
            Note = note
        };

        _context.CalendarShares.Add(share);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Calendar {CalendarId} shared with user {UserId} by {SharedBy} with {Permission} permission",
            calendarId, sharedWithUserId, sharedByUserId, permission);

        return share;
    }

    public async Task<CalendarShare> UpdateSharePermissionAsync(Guid shareId, CalendarSharePermission newPermission, string userId)
    {
        var share = await _context.CalendarShares
            .Include(s => s.Calendar)
            .FirstOrDefaultAsync(s => s.Id == shareId);

        if (share == null)
        {
            throw new KeyNotFoundException($"Calendar share {shareId} not found");
        }

        // Only the calendar owner or someone with ManageSharing permission can update permissions
        if (share.Calendar!.UserId != userId && share.SharedByUserId != userId)
        {
            var userPermission = await GetUserPermissionAsync(share.CalendarId, userId);
            if (userPermission != CalendarSharePermission.ManageSharing)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify sharing settings");
            }
        }

        share.Permission = newPermission;
        share.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated share {ShareId} permission to {Permission}", shareId, newPermission);

        return share;
    }

    public async Task<bool> RemoveShareAsync(Guid shareId, string userId)
    {
        var share = await _context.CalendarShares
            .Include(s => s.Calendar)
            .FirstOrDefaultAsync(s => s.Id == shareId);

        if (share == null)
        {
            return false;
        }

        // Calendar owner, person who shared it, or person it was shared with can remove the share
        if (share.Calendar!.UserId != userId &&
            share.SharedByUserId != userId &&
            share.SharedWithUserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to remove this share");
        }

        _context.CalendarShares.Remove(share);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed calendar share {ShareId}", shareId);

        return true;
    }

    public async Task<List<CalendarShare>> GetCalendarsSharedWithUserAsync(string userId, bool includeUnaccepted = false)
    {
        var query = _context.CalendarShares
            .Include(s => s.Calendar)
            .Include(s => s.SharedByUser)
            .Where(s => s.SharedWithUserId == userId);

        if (!includeUnaccepted)
        {
            query = query.Where(s => s.IsAccepted);
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CalendarShare>> GetCalendarSharesAsync(Guid calendarId)
    {
        return await _context.CalendarShares
            .Include(s => s.SharedWithUser)
            .Include(s => s.SharedByUser)
            .Where(s => s.CalendarId == calendarId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<CalendarShare?> GetShareByIdAsync(Guid shareId)
    {
        return await _context.CalendarShares
            .Include(s => s.Calendar)
            .Include(s => s.SharedWithUser)
            .Include(s => s.SharedByUser)
            .FirstOrDefaultAsync(s => s.Id == shareId);
    }

    public async Task<bool> AcceptShareAsync(Guid shareId, string userId, string? color = null)
    {
        var share = await _context.CalendarShares
            .FirstOrDefaultAsync(s => s.Id == shareId && s.SharedWithUserId == userId);

        if (share == null)
        {
            return false;
        }

        share.IsAccepted = true;
        share.AcceptedAt = DateTime.UtcNow;
        share.Color = color;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} accepted calendar share {ShareId}", userId, shareId);

        return true;
    }

    public async Task<bool> DeclineShareAsync(Guid shareId, string userId)
    {
        var share = await _context.CalendarShares
            .FirstOrDefaultAsync(s => s.Id == shareId && s.SharedWithUserId == userId);

        if (share == null)
        {
            return false;
        }

        share.IsAccepted = false;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} declined calendar share {ShareId}", userId, shareId);

        return true;
    }

    public async Task<bool> HasPermissionAsync(Guid calendarId, string userId, CalendarSharePermission requiredPermission)
    {
        // Calendar owner has all permissions
        var isOwner = await _context.Calendars
            .AnyAsync(c => c.Id == calendarId && c.UserId == userId);

        if (isOwner)
        {
            return true;
        }

        // Check share permission
        var share = await _context.CalendarShares
            .FirstOrDefaultAsync(s => s.CalendarId == calendarId &&
                                     s.SharedWithUserId == userId &&
                                     s.IsAccepted);

        return share != null && share.Permission >= requiredPermission;
    }

    public async Task<CalendarSharePermission?> GetUserPermissionAsync(Guid calendarId, string userId)
    {
        // Calendar owner has ManageSharing permission
        var isOwner = await _context.Calendars
            .AnyAsync(c => c.Id == calendarId && c.UserId == userId);

        if (isOwner)
        {
            return CalendarSharePermission.ManageSharing;
        }

        // Check share permission
        var share = await _context.CalendarShares
            .FirstOrDefaultAsync(s => s.CalendarId == calendarId &&
                                     s.SharedWithUserId == userId &&
                                     s.IsAccepted);

        return share?.Permission;
    }

    #endregion

    #region Public Calendars

    public async Task<PublicCalendar> SubscribeToPublicCalendarAsync(string userId, string name, string icsUrl,
        PublicCalendarCategory category, string? description = null, string? color = null)
    {
        // Check if already subscribed to this URL
        var existing = await _context.PublicCalendars
            .FirstOrDefaultAsync(pc => pc.UserId == userId && pc.IcsUrl == icsUrl);

        if (existing != null)
        {
            throw new InvalidOperationException("You are already subscribed to this public calendar");
        }

        var publicCalendar = new PublicCalendar
        {
            UserId = userId,
            Name = name,
            Description = description,
            IcsUrl = icsUrl,
            Category = category,
            Color = color ?? "#3498db"
        };

        _context.PublicCalendars.Add(publicCalendar);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} subscribed to public calendar {Name} from {IcsUrl}",
            userId, name, icsUrl);

        // Try to sync immediately
        try
        {
            await SyncPublicCalendarAsync(publicCalendar.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync public calendar {CalendarId} on subscription", publicCalendar.Id);
        }

        return publicCalendar;
    }

    public async Task<bool> UnsubscribeFromPublicCalendarAsync(Guid publicCalendarId, string userId)
    {
        var publicCalendar = await _context.PublicCalendars
            .FirstOrDefaultAsync(pc => pc.Id == publicCalendarId && pc.UserId == userId);

        if (publicCalendar == null)
        {
            return false;
        }

        _context.PublicCalendars.Remove(publicCalendar);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} unsubscribed from public calendar {CalendarId}", userId, publicCalendarId);

        return true;
    }

    public async Task<List<PublicCalendar>> GetUserPublicCalendarsAsync(string userId, bool activeOnly = true)
    {
        var query = _context.PublicCalendars
            .Where(pc => pc.UserId == userId);

        if (activeOnly)
        {
            query = query.Where(pc => pc.IsActive);
        }

        return await query
            .OrderBy(pc => pc.Category)
            .ThenBy(pc => pc.Name)
            .ToListAsync();
    }

    public async Task<PublicCalendar> UpdatePublicCalendarAsync(Guid publicCalendarId, string userId,
        string? name = null, string? color = null, bool? isActive = null)
    {
        var publicCalendar = await _context.PublicCalendars
            .FirstOrDefaultAsync(pc => pc.Id == publicCalendarId && pc.UserId == userId);

        if (publicCalendar == null)
        {
            throw new KeyNotFoundException($"Public calendar {publicCalendarId} not found");
        }

        if (name != null)
        {
            publicCalendar.Name = name;
        }

        if (color != null)
        {
            publicCalendar.Color = color;
        }

        if (isActive.HasValue)
        {
            publicCalendar.IsActive = isActive.Value;
        }

        publicCalendar.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated public calendar {CalendarId}", publicCalendarId);

        return publicCalendar;
    }

    public async Task<int> SyncPublicCalendarAsync(Guid publicCalendarId, string userId)
    {
        var publicCalendar = await _context.PublicCalendars
            .FirstOrDefaultAsync(pc => pc.Id == publicCalendarId && pc.UserId == userId);

        if (publicCalendar == null)
        {
            throw new KeyNotFoundException($"Public calendar {publicCalendarId} not found");
        }

        _logger.LogInformation("Syncing public calendar {CalendarId} from {IcsUrl}",
            publicCalendarId, publicCalendar.IcsUrl);

        // Download ICS feed and import events
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var response = await httpClient.GetAsync(publicCalendar.IcsUrl);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        var events = await _icsImportService.ImportFromStreamAsync(stream);

        // Set the user ID for all imported events
        foreach (var evt in events)
        {
            evt.UserId = userId;
            evt.Title = $"[{publicCalendar.Name}] {evt.Title}"; // Prefix with calendar name
        }

        // Save events to database
        _context.Events.AddRange(events);

        publicCalendar.LastSyncedAt = DateTime.UtcNow;
        publicCalendar.EventCount = events.Count;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Synced {EventCount} events from public calendar {CalendarId}",
            events.Count, publicCalendarId);

        return events.Count;
    }

    public async Task<PublicCalendar?> GetPublicCalendarByIdAsync(Guid publicCalendarId)
    {
        return await _context.PublicCalendars
            .FirstOrDefaultAsync(pc => pc.Id == publicCalendarId);
    }

    #endregion
}
