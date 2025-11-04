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
    public async Task<List<Event>> ImportFromStreamAsync(Stream stream)
    {
        Console.WriteLine("[PstImportService] Starting PST import...");
        var events = new List<Event>();

        try
        {
            // Save stream to temporary file (PST reading requires file path)
            var tempFilePath = Path.GetTempFileName();
            Console.WriteLine($"[PstImportService] Created temp file: {tempFilePath}");

            try
            {
                // Copy stream to temporary file
                using (var fileStream = File.Create(tempFilePath))
                {
                    await stream.CopyToAsync(fileStream);
                }
                Console.WriteLine($"[PstImportService] Copied stream to temp file. Size: {new FileInfo(tempFilePath).Length} bytes");

                // Open PST file with Aspose.Email
                using (var pst = PersonalStorage.FromFile(tempFilePath))
                {
                    Console.WriteLine($"[PstImportService] Opened PST file successfully");
                    // Get the root folder
                    var rootFolder = pst.RootFolder;
                    Console.WriteLine($"[PstImportService] Root folder: {rootFolder.DisplayName}");

                    // Recursively search for calendar folders and extract events
                    ExtractEventsFromFolder(pst, rootFolder, events);
                }
                Console.WriteLine($"[PstImportService] Extracted {events.Count} total events from PST");
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    Console.WriteLine($"[PstImportService] Deleted temp file");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PstImportService] ERROR: {ex.Message}");
            Console.WriteLine($"[PstImportService] Stack trace: {ex.StackTrace}");
            throw new InvalidOperationException($"Error parsing PST file: {ex.Message}", ex);
        }

        Console.WriteLine($"[PstImportService] Returning {events.Count} events");
        return events;
    }

    private void ExtractEventsFromFolder(PersonalStorage pst, FolderInfo folder, List<Event> events)
    {
        Console.WriteLine($"[PstImportService] Examining folder: {folder.DisplayName}");

        // Check if this is a calendar folder
        if (IsCalendarFolder(folder))
        {
            Console.WriteLine($"[PstImportService] ✓ Found calendar folder: {folder.DisplayName}");
            // Extract events from this folder
            var messageInfoCollection = folder.GetContents();
            Console.WriteLine($"[PstImportService] Folder contains {messageInfoCollection.Count} items");

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
                    Console.WriteLine($"[PstImportService] Failed to convert message: {ex.Message}");
                    skippedCount++;
                    continue;
                }
            }

            Console.WriteLine($"[PstImportService] Extracted {eventCount} events from this folder, skipped {skippedCount}");
        }

        // Recursively process subfolders
        if (folder.HasSubFolders)
        {
            var subFolders = folder.GetSubFolders();
            Console.WriteLine($"[PstImportService] Processing {subFolders.Count} subfolders...");
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
                Console.WriteLine($"[PstImportService.ConvertToEvent] Skipping non-appointment: {messageClass}");
                return null; // Not a calendar event
            }

            Console.WriteLine($"[PstImportService.ConvertToEvent] Converting appointment: {mapiMessage.Subject}");

            // Convert to MapiCalendar for easier access to appointment properties
            var appointment = mapiMessage.ToMapiMessageItem() as MapiCalendar;
            if (appointment == null)
            {
                Console.WriteLine($"[PstImportService.ConvertToEvent] Failed to convert to MapiCalendar");
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
                UpdatedAt = DateTime.UtcNow
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

            Console.WriteLine($"[PstImportService.ConvertToEvent] ✓ Successfully converted: '{evt.Title}' with {evt.Attendees?.Count ?? 0} attendees");
            return evt;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PstImportService.ConvertToEvent] ERROR during conversion: {ex.Message}");
            return null;
        }
    }
}
