using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for importing calendar events from PST (Outlook Personal Storage) files
/// </summary>
public interface IPstImportService
{
    /// <summary>
    /// Import calendar events from a PST file stream
    /// </summary>
    /// <param name="stream">Stream containing the PST file data</param>
    /// <returns>List of events extracted from the PST file</returns>
    Task<List<Event>> ImportFromStreamAsync(Stream stream);
}
