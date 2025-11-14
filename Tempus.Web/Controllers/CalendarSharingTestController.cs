using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;

namespace Tempus.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/test/calendar-sharing")]
public class CalendarSharingTestController : ControllerBase
{
    private readonly ICalendarSharingService _sharingService;
    private readonly ICalendarRepository _calendarRepository;
    private readonly ILogger<CalendarSharingTestController> _logger;

    public CalendarSharingTestController(
        ICalendarSharingService sharingService,
        ICalendarRepository calendarRepository,
        ILogger<CalendarSharingTestController> logger)
    {
        _sharingService = sharingService;
        _calendarRepository = calendarRepository;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Test: Share a calendar with another user
    /// POST /api/test/calendar-sharing/share
    /// Body: { "calendarId": "guid", "sharedWithUserId": "user-id", "permission": 1, "note": "optional" }
    /// </summary>
    [HttpPost("share")]
    public async Task<IActionResult> ShareCalendar([FromBody] ShareCalendarRequest request)
    {
        try
        {
            var userId = GetUserId();
            var share = await _sharingService.ShareCalendarAsync(
                request.CalendarId,
                request.SharedWithUserId,
                request.Permission,
                userId,
                request.Note
            );

            return Ok(new
            {
                success = true,
                message = "Calendar shared successfully",
                share = new
                {
                    share.Id,
                    share.CalendarId,
                    share.SharedWithUserId,
                    share.Permission,
                    PermissionName = share.GetPermissionDisplayName(),
                    share.Note,
                    share.IsAccepted,
                    share.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing calendar");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Get calendars shared with current user
    /// GET /api/test/calendar-sharing/shared-with-me
    /// </summary>
    [HttpGet("shared-with-me")]
    public async Task<IActionResult> GetSharedWithMe([FromQuery] bool includeUnaccepted = false)
    {
        try
        {
            var userId = GetUserId();
            var shares = await _sharingService.GetCalendarsSharedWithUserAsync(userId, includeUnaccepted);

            return Ok(new
            {
                success = true,
                count = shares.Count,
                shares = shares.Select(s => new
                {
                    s.Id,
                    CalendarName = s.Calendar?.Name,
                    s.CalendarId,
                    SharedBy = s.SharedByUser?.UserName,
                    s.SharedByUserId,
                    s.Permission,
                    PermissionName = s.GetPermissionDisplayName(),
                    s.Note,
                    s.IsAccepted,
                    s.Color,
                    s.CreatedAt,
                    s.AcceptedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shared calendars");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Get shares for a specific calendar
    /// GET /api/test/calendar-sharing/calendar/{calendarId}/shares
    /// </summary>
    [HttpGet("calendar/{calendarId}/shares")]
    public async Task<IActionResult> GetCalendarShares(Guid calendarId)
    {
        try
        {
            var shares = await _sharingService.GetCalendarSharesAsync(calendarId);

            return Ok(new
            {
                success = true,
                count = shares.Count,
                shares = shares.Select(s => new
                {
                    s.Id,
                    SharedWith = s.SharedWithUser?.UserName,
                    s.SharedWithUserId,
                    SharedBy = s.SharedByUser?.UserName,
                    s.SharedByUserId,
                    s.Permission,
                    PermissionName = s.GetPermissionDisplayName(),
                    s.Note,
                    s.IsAccepted,
                    s.CreatedAt,
                    s.AcceptedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calendar shares");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Accept a calendar share
    /// POST /api/test/calendar-sharing/{shareId}/accept
    /// Body: { "color": "#ff0000" } (optional)
    /// </summary>
    [HttpPost("{shareId}/accept")]
    public async Task<IActionResult> AcceptShare(Guid shareId, [FromBody] AcceptShareRequest? request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _sharingService.AcceptShareAsync(shareId, userId, request?.Color);

            return Ok(new
            {
                success = result,
                message = result ? "Calendar share accepted" : "Failed to accept share"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting share");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Check permission on a calendar
    /// GET /api/test/calendar-sharing/calendar/{calendarId}/permission
    /// </summary>
    [HttpGet("calendar/{calendarId}/permission")]
    public async Task<IActionResult> CheckPermission(Guid calendarId, [FromQuery] CalendarSharePermission requiredPermission)
    {
        try
        {
            var userId = GetUserId();
            var hasPermission = await _sharingService.HasPermissionAsync(calendarId, userId, requiredPermission);
            var userPermission = await _sharingService.GetUserPermissionAsync(calendarId, userId);

            return Ok(new
            {
                success = true,
                calendarId,
                userId,
                requiredPermission,
                hasPermission,
                userPermission,
                userPermissionName = userPermission?.ToString() ?? "None"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Subscribe to a public calendar
    /// POST /api/test/calendar-sharing/public/subscribe
    /// Body: { "name": "US Holidays", "icsUrl": "https://...", "category": 0, "description": "...", "color": "#..." }
    /// </summary>
    [HttpPost("public/subscribe")]
    public async Task<IActionResult> SubscribeToPublicCalendar([FromBody] SubscribePublicCalendarRequest request)
    {
        try
        {
            var userId = GetUserId();
            var publicCalendar = await _sharingService.SubscribeToPublicCalendarAsync(
                userId,
                request.Name,
                request.IcsUrl,
                request.Category,
                request.Description,
                request.Color
            );

            return Ok(new
            {
                success = true,
                message = "Subscribed to public calendar",
                calendar = new
                {
                    publicCalendar.Id,
                    publicCalendar.Name,
                    publicCalendar.Description,
                    publicCalendar.IcsUrl,
                    publicCalendar.Category,
                    CategoryName = publicCalendar.GetCategoryDisplayName(),
                    publicCalendar.Color,
                    publicCalendar.IsActive,
                    publicCalendar.EventCount,
                    publicCalendar.LastSyncedAt,
                    publicCalendar.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to public calendar");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Get public calendar subscriptions
    /// GET /api/test/calendar-sharing/public/subscriptions
    /// </summary>
    [HttpGet("public/subscriptions")]
    public async Task<IActionResult> GetPublicCalendars([FromQuery] bool activeOnly = true)
    {
        try
        {
            var userId = GetUserId();
            var calendars = await _sharingService.GetUserPublicCalendarsAsync(userId, activeOnly);

            return Ok(new
            {
                success = true,
                count = calendars.Count,
                calendars = calendars.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.IcsUrl,
                    c.Category,
                    CategoryName = c.GetCategoryDisplayName(),
                    c.Color,
                    c.IsActive,
                    c.EventCount,
                    c.LastSyncedAt,
                    c.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public calendars");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test: Get my calendars (for testing sharing)
    /// GET /api/test/calendar-sharing/my-calendars
    /// </summary>
    [HttpGet("my-calendars")]
    public async Task<IActionResult> GetMyCalendars()
    {
        try
        {
            var userId = GetUserId();
            var calendars = await _calendarRepository.GetAllAsync(userId);

            return Ok(new
            {
                success = true,
                count = calendars.Count,
                calendars = calendars.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Color,
                    c.IsDefault,
                    c.IsVisible
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calendars");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}

public record ShareCalendarRequest(
    Guid CalendarId,
    string SharedWithUserId,
    CalendarSharePermission Permission,
    string? Note = null
);

public record AcceptShareRequest(string? Color = null);

public record SubscribePublicCalendarRequest(
    string Name,
    string IcsUrl,
    PublicCalendarCategory Category,
    string? Description = null,
    string? Color = null
);
