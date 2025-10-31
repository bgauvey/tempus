using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for generating analytics reports in various formats
/// </summary>
public interface IAnalyticsReportService
{
    /// <summary>
    /// Generate a comprehensive analytics report in PDF format
    /// </summary>
    Task<byte[]> GeneratePdfReportAsync(CalendarAnalytics analytics, string userName);

    /// <summary>
    /// Generate analytics data in CSV format
    /// </summary>
    Task<byte[]> GenerateCsvReportAsync(CalendarAnalytics analytics);

    /// <summary>
    /// Generate analytics data in Excel format
    /// </summary>
    Task<byte[]> GenerateExcelReportAsync(CalendarAnalytics analytics, string userName);
}
