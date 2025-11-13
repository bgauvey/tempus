using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tempus.Core.Enums;
using Tempus.Core.Interfaces;
using Tempus.Core.Models;
using Tempus.Infrastructure.Data;

namespace Tempus.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IDbContextFactory<TempusDbContext> _contextFactory;
    private readonly ILogger<AttachmentService> _logger;
    private readonly string _uploadBasePath;
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB

    // Allowed file types (MIME types)
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",
        "text/csv",
        "application/rtf",
        // Images
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp",
        "image/svg+xml",
        // Archives
        "application/zip",
        "application/x-zip-compressed",
        "application/x-rar-compressed",
        "application/x-7z-compressed",
        // Other
        "application/json",
        "application/xml",
        "text/xml"
    };

    public AttachmentService(IDbContextFactory<TempusDbContext> contextFactory, ILogger<AttachmentService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;

        // Set upload base path - this will be relative to the web root
        // The actual physical path will need to be resolved by the consuming application
        _uploadBasePath = Path.Combine("wwwroot", "uploads", "events");
    }

    public async Task<EventAttachment> UploadFileAsync(Guid eventId, Stream fileStream, string fileName, string contentType, string userId)
    {
        try
        {
            _logger.LogInformation("Starting file upload for event {EventId}, file: {FileName}", eventId, fileName);

            // Validate file size
            if (fileStream.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB");
            }

            // Validate content type
            if (!AllowedContentTypes.Contains(contentType))
            {
                throw new InvalidOperationException($"File type '{contentType}' is not allowed");
            }

            // Validate event exists
            await using var context = await _contextFactory.CreateDbContextAsync();
            var eventExists = await context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
            {
                throw new InvalidOperationException($"Event with ID {eventId} not found");
            }

            // Create directory structure: uploads/events/{eventId}/
            var eventDirectory = Path.Combine(_uploadBasePath, eventId.ToString());
            Directory.CreateDirectory(eventDirectory);

            // Generate unique file name to avoid collisions
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var uniqueFileName = $"{fileNameWithoutExtension}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(eventDirectory, uniqueFileName);

            // Save file to disk
            using (var fileStreamDest = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamDest);
            }

            _logger.LogInformation("File saved to disk: {FilePath}", filePath);

            // Create database record
            var attachment = new EventAttachment
            {
                EventId = eventId,
                Type = DetermineAttachmentType(contentType),
                FileName = fileName,
                FilePath = Path.Combine("uploads", "events", eventId.ToString(), uniqueFileName), // Relative path
                ContentType = contentType,
                FileSize = fileStream.Length,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };

            context.EventAttachments.Add(attachment);
            await context.SaveChangesAsync();

            _logger.LogInformation("File attachment record created with ID: {AttachmentId}", attachment.Id);

            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for event {EventId}", fileName, eventId);
            throw;
        }
    }

    public async Task<EventAttachment> AddExternalLinkAsync(Guid eventId, string url, string? title, ExternalLinkType? linkType, string userId, string? description = null)
    {
        try
        {
            _logger.LogInformation("Adding external link for event {EventId}, URL: {Url}", eventId, url);

            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException("Invalid URL format. URL must start with http:// or https://");
            }

            // Validate event exists
            await using var context = await _contextFactory.CreateDbContextAsync();
            var eventExists = await context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
            {
                throw new InvalidOperationException($"Event with ID {eventId} not found");
            }

            // Auto-detect link type if not provided
            if (linkType == null)
            {
                linkType = DetectLinkType(url);
            }

            // Use URL as title if no title provided
            var linkTitle = string.IsNullOrWhiteSpace(title) ? uri.Host : title;

            // Create database record
            var attachment = new EventAttachment
            {
                EventId = eventId,
                Type = AttachmentType.Link,
                ExternalUrl = url,
                LinkTitle = linkTitle,
                ExternalLinkType = linkType,
                Description = description,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };

            context.EventAttachments.Add(attachment);
            await context.SaveChangesAsync();

            _logger.LogInformation("External link attachment created with ID: {AttachmentId}", attachment.Id);

            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding external link {Url} for event {EventId}", url, eventId);
            throw;
        }
    }

    public async Task<List<EventAttachment>> GetAttachmentsForEventAsync(Guid eventId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.EventAttachments
                .Where(a => a.EventId == eventId)
                .OrderBy(a => a.UploadedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for event {EventId}", eventId);
            throw;
        }
    }

    public async Task<EventAttachment?> GetAttachmentByIdAsync(Guid attachmentId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.EventAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<Stream?> GetFileStreamAsync(Guid attachmentId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var attachment = await context.EventAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null || attachment.Type == AttachmentType.Link)
            {
                _logger.LogWarning("Attachment {AttachmentId} not found or is not a file", attachmentId);
                return null;
            }

            if (string.IsNullOrEmpty(attachment.FilePath))
            {
                _logger.LogWarning("File path is empty for attachment {AttachmentId}", attachmentId);
                return null;
            }

            var fullPath = Path.Combine("wwwroot", attachment.FilePath);

            if (!File.Exists(fullPath))
            {
                _logger.LogError("File not found at path: {FilePath} for attachment {AttachmentId}", fullPath, attachmentId);
                return null;
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file stream for attachment {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId, string userId)
    {
        try
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} by user {UserId}", attachmentId, userId);

            await using var context = await _contextFactory.CreateDbContextAsync();
            var attachment = await context.EventAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
            {
                _logger.LogWarning("Attachment {AttachmentId} not found", attachmentId);
                return false;
            }

            // Verify user has permission (optional - implement based on your authorization logic)
            // For now, we'll allow any user to delete, but you might want to check if userId matches event owner

            // If it's a file, delete from disk
            if (attachment.Type != AttachmentType.Link && !string.IsNullOrEmpty(attachment.FilePath))
            {
                var fullPath = Path.Combine("wwwroot", attachment.FilePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted from disk: {FilePath}", fullPath);
                }
                else
                {
                    _logger.LogWarning("File not found at path: {FilePath}", fullPath);
                }
            }

            // Delete database record
            context.EventAttachments.Remove(attachment);
            await context.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} deleted successfully", attachmentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            throw;
        }
    }

    public async Task<string> GetFilePathAsync(Guid attachmentId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var attachment = await context.EventAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath))
            {
                throw new InvalidOperationException($"Attachment {attachmentId} not found or has no file path");
            }

            return Path.Combine("wwwroot", attachment.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file path for attachment {AttachmentId}", attachmentId);
            throw;
        }
    }

    private static AttachmentType DetermineAttachmentType(string contentType)
    {
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return AttachmentType.Image;
        }

        return AttachmentType.File;
    }

    private static ExternalLinkType DetectLinkType(string url)
    {
        var lowerUrl = url.ToLowerInvariant();

        if (lowerUrl.Contains("onenote.com") || lowerUrl.Contains("onenote.office"))
            return ExternalLinkType.OneNote;

        if (lowerUrl.Contains("evernote.com"))
            return ExternalLinkType.Evernote;

        if (lowerUrl.Contains("docs.google.com"))
            return ExternalLinkType.GoogleDocs;

        if (lowerUrl.Contains("sheets.google.com"))
            return ExternalLinkType.GoogleSheets;

        if (lowerUrl.Contains("slides.google.com"))
            return ExternalLinkType.GoogleSlides;

        if (lowerUrl.Contains("notion.so") || lowerUrl.Contains("notion.site"))
            return ExternalLinkType.Notion;

        if (lowerUrl.Contains("confluence.atlassian.com") || lowerUrl.Contains("confluence."))
            return ExternalLinkType.Confluence;

        if (lowerUrl.Contains("sharepoint.com"))
            return ExternalLinkType.SharePoint;

        if (lowerUrl.Contains("dropbox.com"))
            return ExternalLinkType.Dropbox;

        if (lowerUrl.Contains("onedrive.live.com") || lowerUrl.Contains("1drv.ms"))
            return ExternalLinkType.OneDrive;

        if (lowerUrl.Contains("box.com"))
            return ExternalLinkType.Box;

        if (lowerUrl.Contains("github.com"))
            return ExternalLinkType.GitHub;

        if (lowerUrl.Contains("atlassian.net/browse") || lowerUrl.Contains("jira."))
            return ExternalLinkType.Jira;

        if (lowerUrl.Contains("trello.com"))
            return ExternalLinkType.Trello;

        if (lowerUrl.Contains("asana.com"))
            return ExternalLinkType.Asana;

        if (lowerUrl.Contains("slack.com"))
            return ExternalLinkType.Slack;

        if (lowerUrl.Contains("teams.microsoft.com"))
            return ExternalLinkType.Teams;

        if (lowerUrl.Contains("zoom.us"))
            return ExternalLinkType.Zoom;

        if (lowerUrl.Contains("meet.google.com"))
            return ExternalLinkType.Meet;

        return ExternalLinkType.Other;
    }
}
