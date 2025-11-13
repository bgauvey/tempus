using Tempus.Core.Enums;

namespace Tempus.Core.Models;

public class EventAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public AttachmentType Type { get; set; }

    // For file uploads
    public string? FileName { get; set; }
    public string? FilePath { get; set; } // Relative path in storage
    public string? ContentType { get; set; }
    public long? FileSize { get; set; } // in bytes

    // For external links
    public string? ExternalUrl { get; set; }
    public string? LinkTitle { get; set; }
    public ExternalLinkType? ExternalLinkType { get; set; }

    // Common metadata
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = string.Empty; // User ID

    // Helper methods
    public bool IsImage()
    {
        if (Type != AttachmentType.File || string.IsNullOrEmpty(ContentType))
            return false;

        return ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    public string GetDisplayName()
    {
        return Type switch
        {
            AttachmentType.File => FileName ?? "Unknown File",
            AttachmentType.Link => LinkTitle ?? ExternalUrl ?? "External Link",
            AttachmentType.Image => FileName ?? "Image",
            _ => "Attachment"
        };
    }

    public string GetFormattedFileSize()
    {
        if (!FileSize.HasValue || FileSize.Value == 0)
            return "Unknown size";

        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = FileSize.Value;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
