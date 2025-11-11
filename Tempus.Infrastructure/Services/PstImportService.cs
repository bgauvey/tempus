using Microsoft.Extensions.Logging;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Core.Enums;
using Aspose.Email.Storage.Pst;
using Aspose.Email.Mapi;

namespace Tempus.Infrastructure.Services;

/// <summary>
/// Service for importing calendar events from PST (Outlook Personal Storage) files using Aspose.Email
/// </summary>
public class PstImportService : IPstImportService
{
    private readonly ILogger<PstImportService> _logger;

    public PstImportService(ILogger<PstImportService> logger)
    {
        _logger = logger;
    }
    public async Task<List<Event>> ImportFromStreamAsync(Stream stream)
    {
        _logger.LogDebug("Starting PST import...");
        var events = new List<Event>();

        try
        {
            // Save stream to temporary file (PST reading requires file path)
            var tempFilePath = Path.GetTempFileName();
            _logger.LogDebug("Created temp file: {TempFile}", tempFilePath);

            try
            {
                // Copy stream to temporary file
                using (var fileStream = File.Create(tempFilePath))
                {
                    await stream.CopyToAsync(fileStream);
                }
                _logger.LogDebug("Copied stream to temp file. Size: {Size} bytes", new FileInfo(tempFilePath).Length);

                // Open PST file with Aspose.Email
                using (var pst = PersonalStorage.FromFile(tempFilePath))
                {
                    _logger.LogDebug("Opened PST file successfully");
                    // Get the root folder
                    var rootFolder = pst.RootFolder;
                    _logger.LogDebug("Root folder: {FolderName}", rootFolder.DisplayName);

                    // Recursively search for calendar folders and extract events
                    ExtractEventsFromFolder(pst, rootFolder, events);
                }
                _logger.LogDebug("Extracted {EventCount} total events from PST", events.Count);
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    _logger.LogDebug("Deleted temp file");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR parsing PST file");
            throw new InvalidOperationException($"Error parsing PST file: {ex.Message}", ex);
        }

        _logger.LogDebug("Returning {EventCount} events", events.Count);
        return events;
    }

    private void ExtractEventsFromFolder(PersonalStorage pst, FolderInfo folder, List<Event> events)
    {
        _logger.LogDebug("Examining folder: {FolderName}", folder.DisplayName);

        // Check if this is a calendar folder
        if (IsCalendarFolder(folder))
        {
            _logger.LogDebug("✓ Found calendar folder: {FolderName}", folder.DisplayName);
            // Extract events from this folder
            var messageInfoCollection = folder.GetContents();
            _logger.LogDebug("Folder contains {ItemCount} items", messageInfoCollection.Count);

            int eventCount = 0;
            int skippedCount = 0;

            foreach (var messageInfo in messageInfoCollection)
            {
                try
                {
                    var mapiMessage = pst.ExtractMessage(messageInfo);
                    var evt = ConvertToEvent(mapiMessage);
                    if (evt != null)
                    {
                        events.Add(evt);
                        eventCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Skip messages that can't be converted
                    _logger.LogWarning(ex, "Failed to convert message");
                    skippedCount++;
                    continue;
                }
            }

            _logger.LogDebug("Extracted {EventCount} events from this folder, skipped {SkippedCount}", eventCount, skippedCount);
        }

        // Recursively process subfolders
        if (folder.HasSubFolders)
        {
            var subFolders = folder.GetSubFolders();
            _logger.LogDebug("Processing {SubFolderCount} subfolders...", subFolders.Count);
            foreach (var subfolder in subFolders)
            {
                ExtractEventsFromFolder(pst, subfolder, events);
            }
        }
    }

    private bool IsCalendarFolder(FolderInfo folder)
    {
        // Check if folder name suggests it's a calendar folder
        var folderName = folder.DisplayName?.ToLower() ?? "";
        return folderName.Contains("calendar") ||
               folderName.Contains("kalender") ||
               folderName.Contains("calendrier") ||
               folderName.Contains("agenda") ||
               folder.ContainerClass == "IPF.Appointment"; // Standard Outlook calendar class
    }

    private Event? ConvertToEvent(MapiMessage mapiMessage)
    {
        try
        {
            // Check if this is a calendar item (appointment)
            var messageClass = mapiMessage.MessageClass ?? "";
            if (!messageClass.StartsWith("IPM.Appointment", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Skipping non-appointment: {MessageClass}", messageClass);
                return null; // Not a calendar event
            }

            _logger.LogDebug("Converting appointment: {Subject}", mapiMessage.Subject);

            // Convert to MapiCalendar for easier access to appointment properties
            var appointment = mapiMessage.ToMapiMessageItem() as MapiCalendar;
            if (appointment == null)
            {
                _logger.LogDebug("Failed to convert to MapiCalendar");
                return null;
            }

            // Extract event properties
            var subject = appointment.Subject ?? "Untitled Event";
            var startTime = appointment.StartDate;
            var endTime = appointment.EndDate;
            var location = appointment.Location ?? "";
            var body = mapiMessage.Body ?? "";

            // Validate dates
            if (startTime == DateTime.MinValue || startTime == default)
            {
                startTime = DateTime.UtcNow;
            }
            if (endTime == DateTime.MinValue || endTime == default || endTime < startTime)
            {
                endTime = startTime.AddHours(1); // Default 1 hour duration
            }

            // Ensure dates are in a valid range for database storage
            if (startTime.Year < 1753)
            {
                startTime = new DateTime(1753, 1, 1);
            }
            if (endTime.Year < 1753)
            {
                endTime = startTime.AddHours(1);
            }

            // Ensure events have at least some duration (fix zero-duration events)
            // Zero-duration events may not render properly in the calendar
            if (endTime == startTime)
            {
                endTime = startTime.AddHours(1); // Default 1 hour duration
                _logger.LogDebug("Fixed zero-duration event '{Subject}': added 1 hour duration", subject);
            }

            // CRITICAL: Ensure imported times are in UTC for consistent storage
            // PST files typically store times in local timezone
            if (startTime.Kind == DateTimeKind.Local)
            {
                startTime = startTime.ToUniversalTime();
                endTime = endTime.ToUniversalTime();
                _logger.LogDebug("Converted times from Local to UTC");
            }
            else if (startTime.Kind == DateTimeKind.Unspecified)
            {
                // Treat unspecified times as UTC to be safe
                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
                _logger.LogDebug("Marked unspecified times as UTC");
            }

            // Create event object
            var evt = new Event
            {
                Title = subject,
                StartTime = startTime,
                EndTime = endTime,
                Location = location,
                Description = body,
                IsAllDay = false,
                EventType = EventType.Meeting,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TimeZoneId = "UTC" // All imported events stored in UTC
            };

            // Extract attendees if present - using recipients from the mapiMessage
            if (mapiMessage.Recipients != null && mapiMessage.Recipients.Count > 0)
            {
                evt.Attendees = new List<Attendee>();
                foreach (var recipient in mapiMessage.Recipients)
                {
                    // Skip attendees without email addresses (required field)
                    var email = recipient.EmailAddress;
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        continue;
                    }

                    var attendee = new Attendee
                    {
                        Name = recipient.DisplayName ?? email,
                        Email = email,
                        IsOrganizer = false  // Will be set based on organizer email if available
                    };

                    evt.Attendees.Add(attendee);
                }
            }

            _logger.LogDebug("✓ Successfully converted: '{Title}' with {AttendeeCount} attendees", evt.Title, evt.Attendees?.Count ?? 0);
            return evt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR during conversion");
            return null;
        }
    }
}
