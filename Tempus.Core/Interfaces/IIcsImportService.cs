using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IIcsImportService
{
    Task<List<Event>> ImportFromStreamAsync(Stream fileStream);
    Task<string> ExportToIcsAsync(List<Event> events);
}
