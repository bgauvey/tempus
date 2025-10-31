using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service for trend analysis and predictive forecasting
/// </summary>
public interface ITrendForecastService
{
    /// <summary>
    /// Generate comprehensive trend analysis and forecasts
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="historicalDays">Days of historical data to analyze</param>
    /// <param name="forecastDays">Days to forecast into the future</param>
    Task<TrendAnalysis> GenerateTrendAnalysisAsync(string userId, int historicalDays = 90, int forecastDays = 30);

    /// <summary>
    /// Get trend for a specific metric
    /// </summary>
    Task<MetricTrend> GetMetricTrendAsync(string userId, TrendMetric metric, int days = 90);

    /// <summary>
    /// Get prediction for a specific metric
    /// </summary>
    Task<MetricPrediction> GetMetricPredictionAsync(string userId, TrendMetric metric, int historicalDays = 90, int forecastDays = 30);

    /// <summary>
    /// Forecast workload for upcoming periods
    /// </summary>
    Task<List<WorkloadForecast>> ForecastWorkloadAsync(string userId, int forecastDays = 30);

    /// <summary>
    /// Detect patterns in calendar usage
    /// </summary>
    Task<List<string>> DetectPatternsAsync(string userId, int days = 90);
}
