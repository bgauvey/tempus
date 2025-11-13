using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tempus.Core.Interfaces;

namespace Tempus.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(IAttachmentService attachmentService, ILogger<AttachmentsController> logger)
    {
        _attachmentService = attachmentService;
        _logger = logger;
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        try
        {
            var attachment = await _attachmentService.GetAttachmentByIdAsync(id);
            if (attachment == null)
            {
                _logger.LogWarning("Attachment {AttachmentId} not found for download", id);
                return NotFound("Attachment not found");
            }

            // Only file and image types can be downloaded
            if (attachment.Type == Core.Enums.AttachmentType.Link)
            {
                _logger.LogWarning("Attempted to download link attachment {AttachmentId}", id);
                return BadRequest("Links cannot be downloaded");
            }

            var fileStream = await _attachmentService.GetFileStreamAsync(id);
            if (fileStream == null)
            {
                _logger.LogError("File stream not found for attachment {AttachmentId}", id);
                return NotFound("File not found");
            }

            var contentType = attachment.ContentType ?? "application/octet-stream";
            var fileName = attachment.FileName ?? $"attachment_{id}";

            _logger.LogInformation("Downloading attachment {AttachmentId}: {FileName}", id, fileName);

            return File(fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);
            return StatusCode(500, "Error downloading file");
        }
    }

    [HttpGet("{id}/preview")]
    public async Task<IActionResult> PreviewFile(Guid id)
    {
        try
        {
            var attachment = await _attachmentService.GetAttachmentByIdAsync(id);
            if (attachment == null)
            {
                _logger.LogWarning("Attachment {AttachmentId} not found for preview", id);
                return NotFound("Attachment not found");
            }

            // Only images can be previewed inline
            if (!attachment.IsImage())
            {
                _logger.LogWarning("Attempted to preview non-image attachment {AttachmentId}", id);
                return BadRequest("Only images can be previewed");
            }

            var fileStream = await _attachmentService.GetFileStreamAsync(id);
            if (fileStream == null)
            {
                _logger.LogError("File stream not found for attachment {AttachmentId}", id);
                return NotFound("File not found");
            }

            var contentType = attachment.ContentType ?? "image/jpeg";

            _logger.LogInformation("Previewing image attachment {AttachmentId}", id);

            // Return file with inline disposition for browser preview
            return File(fileStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing attachment {AttachmentId}", id);
            return StatusCode(500, "Error loading preview");
        }
    }
}
