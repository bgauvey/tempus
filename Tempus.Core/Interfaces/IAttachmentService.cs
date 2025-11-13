using Tempus.Core.Enums;
using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IAttachmentService
{
    Task<EventAttachment> UploadFileAsync(Guid eventId, Stream fileStream, string fileName, string contentType, string userId);
    Task<EventAttachment> AddExternalLinkAsync(Guid eventId, string url, string? title, ExternalLinkType? linkType, string userId, string? description = null);
    Task<List<EventAttachment>> GetAttachmentsForEventAsync(Guid eventId);
    Task<EventAttachment?> GetAttachmentByIdAsync(Guid attachmentId);
    Task<Stream?> GetFileStreamAsync(Guid attachmentId);
    Task<bool> DeleteAttachmentAsync(Guid attachmentId, string userId);
    Task<string> GetFilePathAsync(Guid attachmentId);
}
