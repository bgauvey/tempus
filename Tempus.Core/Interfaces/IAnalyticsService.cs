using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for generating calendar analytics and insights
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Generate comprehensive calendar analytics for a date range
    /// </summary>
    Task<CalendarAnalytics> GenerateAnalyticsAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get quick summary statistics
    /// </summary>
    Task<CalendarAnalytics> GetQuickStatsAsync(string userId, int days = 30);

    /// <summary>
    /// Calculate calendar health score
    /// </summary>
    Task<double> CalculateCalendarHealthScoreAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get time usage breakdown
    /// </summary>
    Task<TimeUsageMetrics> GetTimeUsageMetricsAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get meeting analytics
    /// </summary>
    Task<MeetingAnalytics> GetMeetingAnalyticsAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get productivity metrics
    /// </summary>
    Task<ProductivityMetrics> GetProductivityMetricsAsync(string userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get recommendations based on calendar patterns
    /// </summary>
    Task<List<string>> GetRecommendationsAsync(string userId, DateTime startDate, DateTime endDate);
}
