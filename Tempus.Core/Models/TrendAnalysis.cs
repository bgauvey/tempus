namespace Tempus.Core.Models;

/// <summary>
/// Represents a data point in a time series
/// </summary>
public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Trend direction indicator
/// </summary>
public enum TrendDirection
{
    Increasing,
    Decreasing,
    Stable,
    Volatile
}

/// <summary>
/// Types of metrics that can be forecasted
/// </summary>
public enum TrendMetric
{
    TotalEvents,
    MeetingCount,
    MeetingCost,
    FocusTimeHours,
    ScheduledHours,
    CalendarHealthScore,
    TaskCompletionRate,
    BackToBackMeetings
}

/// <summary>
/// Historical trend analysis for a specific metric
/// </summary>
public class MetricTrend
{
    public TrendMetric Metric { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public List<TrendDataPoint> HistoricalData { get; set; } = new();
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

/// <summary>
/// Future predictions for a specific metric
/// </summary>
public class MetricPrediction
{
    public TrendMetric Metric { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public List<TrendDataPoint> PredictedData { get; set; } = new();
    public double PredictedValue { get; set; }
    public double ConfidenceLevel { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Comprehensive trend analysis and forecasting results
/// </summary>
public class TrendAnalysis
{
    public string UserId { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int HistoricalDays { get; set; }
    public int ForecastDays { get; set; }

    // Historical trends
    public List<MetricTrend> Trends { get; set; } = new();

    // Future predictions
    public List<MetricPrediction> Predictions { get; set; } = new();

    // Overall insights
    public List<string> KeyInsights { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    // Pattern detection
    public string WorkloadPattern { get; set; } = string.Empty;
    public List<string> DetectedPatterns { get; set; } = new();
}

/// <summary>
/// Workload forecast for specific time periods
/// </summary>
public class WorkloadForecast
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PredictedEventCount { get; set; }
    public double PredictedHours { get; set; }
    public double PredictedMeetingCost { get; set; }
    public string WorkloadLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
